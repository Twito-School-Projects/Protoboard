
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TreeDict = System.Collections.Generic.Dictionary<Hole, CircuitTree>;
using UnityEngine;

public enum TreeType { Battery, Arduino, DisconnectedBattery, DisconnectedArduino }

public class CircuitManager : Singleton<CircuitManager>
{
    //Rails
    public TreeDict Trees = new();
    
    //Rails
    public TreeDict GPIOTrees = new();

    
    //Rails - Disconnected
    public TreeDict DisconnectedTrees = new();
    
    //Rails - Disconnected
    public TreeDict DisconnectedGPIOTrees = new();

    private void OnEnable()
    {
        Wire.OnWireDeleted += OnWireDeleted;
    }
    
    private void OnDisable()
    {
        Wire.OnWireDeleted -= OnWireDeleted;
    }
    
    private void OnWireDeleted(Wire arg1, ConnectionPoint start, ConnectionPoint end)
    {
        DisconnectNodes(start, end);
    }

    public void DisconnectNodes(ConnectionPoint start, ConnectionPoint end)
    {
        var startNode = FindNodeInTree(TreeType.Battery, start);
        if (startNode == null) return;
        //removing the child node (not deleting it)
        var endNode = startNode.Children.FirstOrDefault(x => x.Data == end);
        if (endNode == null) return;
        
        // Debug.Log("State: " % Colorize.Gold + (endNode.Data.powered ? "Powered" : "Unpowered"));

        PropagatePower(endNode);
        startNode.RemoveChildNode(endNode);
        
        Create(TreeType.DisconnectedBattery, endNode.Data as Hole);
    }


    public bool IsEmpty(TreeType type, Hole parent)
    {
        if (parent.charge != Charge.Positive && parent.type != ConnectionPointType.Rail)
        {
            Debug.Log("Wrong Typ of parent");
            return false;
        }
        
        var tree = type == TreeType.Arduino ? GPIOTrees : Trees;
        return tree.TryGetValue(parent, out var circuit) && circuit.IsEmpty;
    }

    [CanBeNull]
    public CircuitTree GetTree(Hole parent)
    {
        return Trees.GetValueOrDefault(parent);
    }

    public bool IsChildPartOfDisconnected(ConnectionPoint child, out CircuitNode childNode)
    {
        foreach (var tree in DisconnectedTrees.Values)
        {
            var node = tree.DepthFirstSearch(child);
            if (node != null)
            {
                childNode = node;
                return true;
            }
        }
        
        childNode = null;
        return false;
    }

    [CanBeNull]
    public CircuitNode FindNodeInTree(TreeDict dict, ConnectionPoint point)
    {
        var tree = dict.Values.FirstOrDefault(x => x.DepthFirstSearch(point) != null);
        return tree?.DepthFirstSearch(point);
    }
    
    [CanBeNull]
    public CircuitNode FindNodeInTree(TreeType type, ConnectionPoint point)
    {
        var dict = type switch
        {
            TreeType.Battery => Trees,
            TreeType.Arduino => GPIOTrees,
            TreeType.DisconnectedBattery => DisconnectedTrees,
            TreeType.DisconnectedArduino => DisconnectedGPIOTrees,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        var tree = dict.Values.FirstOrDefault(x => x.DepthFirstSearch(point) != null);
        return tree?.DepthFirstSearch(point);
    }
    
    [CanBeNull]
    public CircuitNode FindNodeInTrees(ConnectionPoint point)
    {
        List<TreeDict> trees = new List<TreeDict>() {Trees ,GPIOTrees, DisconnectedTrees, DisconnectedGPIOTrees};
        foreach (var d in trees)
        {
            foreach (var t in d.Values)
            {
                var result = t.DepthFirstSearch(point);
                if (result != null)
                    return result;
            }
        }

        return null;
    }
    
    [CanBeNull]
    public CircuitTree FindTree(ConnectionPoint point)
    {
        List<TreeDict> trees = new List<TreeDict>() {Trees ,GPIOTrees, DisconnectedTrees, DisconnectedGPIOTrees};
        foreach (var d in trees)
        {
            foreach (var t in d.Values)
            {
                var result = t.DepthFirstSearch(point);
                if (result != null)
                    return t;
            }
        }

        return null;
    }
    
    public void PropagatePower(CircuitNode node)
    {
        node.Children.ForEach(c =>
        {
            var child = c.Data as Hole; 
            if (child && (child.IsTerminal || child.IsNegativeRail))
            {
                child.SetPowered(node.Data.powered);
            }

            PropagatePower(c);
        });
    }

    public CircuitNode AddChild(ref CircuitNode parent, ref Hole end)
    {
        var c = end as ConnectionPoint;
        return AddChild(ref parent, ref c);
    }
    
    public CircuitNode AddChild(ref CircuitNode parent, ref ConnectionPoint end)
    {
        var node = new CircuitNode(end);
        var endHole = end as Hole;
        Debug.Log(end.type);
        //if the child is a terminal, then add the terminals as well
       if (end.type == ConnectionPointType.Terminal)
        {
            var parentHole = parent.Data as Hole;
            foreach (var adjacentHole in endHole.parentTerminal.holes)
            {
                if (parent.Children.FirstOrDefault(x => x.Data == adjacentHole) != null)
                {
                    Debug.Log("hole already added to tree");
                    continue;
                }
                
                adjacentHole.Resistors.AddRange(endHole.Resistors.Where(x => !adjacentHole.Resistors.Contains(x)));
                adjacentHole.Resistors.AddRange(parentHole.Resistors.Where(x => !adjacentHole.Resistors.Contains(x)));

                var childNode = new CircuitNode(adjacentHole);
                parent.AddChildNode(childNode);
                Debug.Log("adding child node");
            }
        }

        parent.AddChildNode(node);
        parent.Children.ForEach(x => Debug.Log(x.Data.name));
        Debug.Log(parent.Children.Last().Data.name);
        return node;
    }
    
    

    public void ReconnectTree(ConnectionPoint parent, ConnectionPoint child, TreeType type)
    {
        var connectedTree = type == TreeType.Arduino ? GPIOTrees : Trees;
        var disconnectedTree = type == TreeType.Arduino ? DisconnectedGPIOTrees : DisconnectedTrees;
        
        var pNode = FindNodeInTree(connectedTree, parent);
        var cNode = FindNodeInTree(disconnectedTree, child);
        
        pNode.Children.Add(cNode);
        disconnectedTree.Remove(cNode.Data as Hole);
    }
    
    public void HandlePositiveElectrode(ConnectionPoint startConnectionPoint, ConnectionPoint endConnectionPoint)
    {
        if (startConnectionPoint.type != ConnectionPointType.Battery ||
            startConnectionPoint.charge != Charge.Positive) return;
        if (endConnectionPoint.type != ConnectionPointType.Rail) return;
        
        var connectionPoint = (Hole)endConnectionPoint;
                
    }

    public void PrintPaths(CircuitTree tree)
    {
        var paths = tree.getPaths(tree.Root);

        foreach (var path in paths)
        {
            string a = "";
            foreach (var item in path)
            {
                a += item.Data.name + " -> ";
            }
            Debug.Log(a);
        }

    }
    
    public void PrintPath(List<CircuitNode> path)
    {

        var a = new StringBuilder();
        foreach (var item in path)
        {
            a.Append(item.Data.name + " -> ");
        }
        Debug.Log(a);
    

    }

    public void Create(TreeType type, Hole root)
    {
        var dict = type switch
        {
            TreeType.Battery => Trees,
            TreeType.Arduino => GPIOTrees,
            TreeType.DisconnectedBattery => DisconnectedTrees,
            TreeType.DisconnectedArduino => DisconnectedGPIOTrees,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        if (dict.ContainsKey(root))
        {
            Debug.LogWarning("Tree already exists for this root");
            return;
        }
        var tree = new CircuitTree(root);
        dict.Add(root, tree);
    }

    public bool HasValidPath(ConnectionPoint startPoint, ConnectionPoint endPoint)
    {
        if (startPoint == null || endPoint == null) return false;
    
        var tree = FindTree(startPoint);
        if (tree == null) return false;
    
        var startNode = tree.DepthFirstSearch(startPoint);
        var endNode = tree.DepthFirstSearch(endPoint);
    
        // Both points must be in the same tree
        if (startNode == null || endNode == null)
        {
            //Debug.Log("Nodes not in the same tree");
            return false;
        }
    
        // Check if there's a path from a positive source to a negative terminal
        var paths = tree.getPaths(tree.Root);
    
        foreach (var path in paths)
        {
            path.Reverse();
            // Check if this path contains both our start and end points
            bool hasStart = path.Any(node => node.Data == startPoint);
            bool hasEnd = path.Any(node => node.Data == endPoint);
            bool hasNegativeRail = path.Any(node => node.Data.type == ConnectionPointType.Rail && 
                                                    node.Data.charge == Charge.Negative);
            //Debug.Log($"{hasStart} {hasEnd} {hasNegativeRail}");
            if (hasStart && hasEnd && hasNegativeRail)
            {
                // Verify the path goes from positive to negative
                var result =  path.First().Data.charge == Charge.Positive && 
                       path.Last().Data.charge == Charge.Negative;
                
                PrintPath(path);
                return result;
            }
        }
    
        return false;

    }
    
    public void PropagateUp(CircuitNode tailNode)
    {
        if (tailNode.Parent == null)
        {
            Debug.Log("at the positive rail, cannot propagate up");
            return;
        }
        
        tailNode.Data.wasPropagated = true;
        
        tailNode.Parent.Data.wasPropagated = true;
        tailNode.Parent.Children.ForEach(x =>
        {
            var childData = x.Data as Hole;
            var tailData = tailNode.Data as Hole;
            var parentData = tailNode.Parent.Data as Hole;
            
            if (childData.name == tailData.name) return;
            if (childData.column == tailData.column)
            {
                foreach (var resistor in parentData.Resistors)
                {
                    PropagateResistanceUp(tailNode.Parent, resistor);
                }
                childData.wasPropagated = true;
                Debug.Log("Propagating up from " + childData.name + " to " + tailData.name);
            }
        });
        
        PropagateUp(tailNode.Parent);
    }
    
    public void PropagateResistanceUp(CircuitNode root, Resistor resistor)
    {
        var data = root.Data as Hole;
        if(!data.Resistors.Contains((resistor))) data.Resistors.Add(resistor);
        root.Parent.Children.ForEach(c =>
        {
            var child = c.Data as Hole; 
            if (child && (child.IsTerminal || child.IsNegativeRail))
            {
                if (!child.Resistors.Contains(resistor))
                {
                    //might cause a bug in the future
                    PropagateResistanceUp(c, resistor);
                }
            }
        });
    }
}
