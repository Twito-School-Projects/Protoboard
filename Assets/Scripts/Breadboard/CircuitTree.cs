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

    public CircuitNode(ConnectionPoint data)
    {
        Data = data;
    }

    public void AddChildNode(CircuitNode childNode)
    {
        Children.Add(childNode);
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
        Root.Children.AddRange(nodes.Select(x => new CircuitNode(x)));
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
            node.Children.Remove(child);
        }
    }
}