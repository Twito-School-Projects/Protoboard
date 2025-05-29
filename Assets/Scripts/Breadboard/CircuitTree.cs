using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircuitNode
{
    public ConnectionPoint Data { get; set; }
    public List<CircuitNode> Children { get; } = new List<CircuitNode>();

    public CircuitNode(ConnectionPoint data)
    {
        Data = data;
    }
}

public class CircuitTree
{
    public CircuitNode Root { get; set; }

    public CircuitTree(ConnectionPoint data)
    {
        Root = new CircuitNode(data);
    }

    public void AddChildNodesToRoot(List<ConnectionPoint> nodes)
    {
        Root.Children.AddRange(nodes.Select(x => new CircuitNode(x)));
    }
}