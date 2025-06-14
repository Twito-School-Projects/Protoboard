
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
            throw new Exception("Wrong Typ of parent");
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

    public CircuitNode AddChild(CircuitNode parent, ConnectionPoint end)
    {
        var node = new CircuitNode(end);
        var hole = end as Hole;
        
        //if the child is a terminal, then add the terminals as well
        if (end.type == ConnectionPointType.Terminal)
        {
            foreach (var h in hole.parentTerminal.holes)
            {
                if (parent.Children.FirstOrDefault(x => x.Data == h) != null)
                {
                    Debug.Log("hole already added to tree");
                    continue;
                }

                var childNode = new CircuitNode(h);
                parent.AddChildNode(childNode);
            }
        }

        parent.AddChildNode(node);
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
        paths.Reverse();

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
        var tree = new CircuitTree(root);
        dict.Add(root, tree);
    }

    public bool HasValidPath(ConnectionPoint point)
    {
        if (point == null) return false;
        var tree = FindTree(point);
        var node = tree.DepthFirstSearch(point);

        if (node == null)
        {
            Debug.Log("Node does not exist");
            return false;
        }
        
        var paths = tree.getPaths(tree.Root);

        foreach (var path in paths)
        {
            Debug.Log(path.Last().Data.name);
            Debug.Log(path.First().Data.name);

            if (path.First().Data.charge == Charge.Negative)
            {
                return true;
            }
        }

        return false;
    }
}
