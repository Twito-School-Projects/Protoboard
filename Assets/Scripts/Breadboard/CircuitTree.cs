using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;

[Serializable]
public class CircuitNode
{
    public ConnectionPoint Data { get; set; }
    public List<CircuitNode> Children { get; } = new List<CircuitNode>();
    public CircuitNode Parent { get; set; }

    public CircuitNode(ConnectionPoint data)
    {
        Data = data;
    }

    public CircuitNode(ConnectionPoint data, CircuitNode parent)
    {
        Data = data;
        Parent = parent;
    }

    public void AddChildNode(CircuitNode childNode)
    {
        Children.Add(childNode);
        childNode.Data.SetPowered(Data.powered);
    }

    public void RemoveChildNode(CircuitNode childNode)
    {
        Children.Remove(childNode);
        childNode.Data.SetPowered(Data.powered);
    }
}

[Serializable]
public class CircuitTree
{
    public CircuitNode Root { get; set; }
    public bool IsEmpty => Root == null || Root.Children.Count == 0;

    public CircuitTree(ConnectionPoint data)
    {
        Root = new CircuitNode(data);
    }

    public void AddChildNodesToRoot(List<ConnectionPoint> nodes)
    {
        if (Root != null)
        {
            Root.Children.AddRange(nodes.Select(x => new CircuitNode(x, Root)));
        } else
        {
            Debug.Log("Root node does not exist" % Colorize.Red);
        }
    }

    public void AddChildToNode(ConnectionPoint parent, ConnectionPoint child)
    {
        CircuitNode node = DepthFirstSearch(Root, parent);
        if (node != null)
        {
            if (!parent.powered)
            {
                Debug.Log("Not adding to the tree because the parent is not powered");
                return;
            }

            node.AddChildNode(new CircuitNode(child, node));
        }
    }

    public CircuitNode DepthFirstSearch(CircuitNode node, ConnectionPoint target)
    {
        if (node == null || target == null)
            return null;

        if (node.Data.Equals(target))
            return node;

        foreach (var child in node.Children)
        {
            var result = DepthFirstSearch(child, target);
            if (result != null)
                return result;
        }

        return null;
    }

    public void RemoveChildren(CircuitNode node)
    {
        if (node == null)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            RemoveChildren(child);
            ComponentTracker.Instance.breadboard.PropogatePower(node);
            node.Children.Remove(child);
        }
    }

    private List<List<CircuitNode>> getPaths0(CircuitNode pos)
    {
        List<List<CircuitNode>> retLists = new();

        if (pos.Children.Count == 0)
        {
            List<CircuitNode> leafList = new();
            leafList.Add(pos);
            retLists.Add(leafList);
        }
        else
        {
            foreach (var node in pos.Children)
            {
                List<List<CircuitNode>> nodeLists = getPaths0(node);

                foreach (List<CircuitNode> nodeList in nodeLists)
                {
                    nodeList.Add(pos);
                    retLists.Add(nodeList);
                }
            }
        }

        return retLists;
    }

    public List<List<CircuitNode>> getPaths(CircuitNode head)
    {
        if (head == null)
        {
            return new();
        }
        else
        {
            return getPaths0(head);
        }
    }
}