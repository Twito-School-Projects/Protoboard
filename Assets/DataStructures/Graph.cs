using System;
using System.Collections.Generic;

// (Node<T> class remains the same as before)
public class Node<T>
{
    public T Value { get; set; }
    public List<Node<T>> Neighbors { get; private set; }

    public Node(T value)
    {
        Value = value;
        Neighbors = new List<Node<T>>();
    }

    public void AddNeighbor(Node<T> neighbor)
    {
        if (neighbor != null && !Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
        }
    }

    // Optional: Remove a neighbor
    public bool RemoveNeighbor(Node<T> neighbor)
    {
        return Neighbors.Remove(neighbor);
    }

    public override string ToString()
    {
        return $"Node: {Value}";
    }

    // Useful for HashSet/Dictionary equality checks if Value type is not unique
    public override bool Equals(object obj)
    {
        if (obj is Node<T> otherNode)
        {
            // This assumes T's Equals method is sufficient for identifying unique nodes
            return EqualityComparer<T>.Default.Equals(this.Value, otherNode.Value);
        }
        return false;
    }

    // Must override GetHashCode if overriding Equals
    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(this.Value);
    }
}

// (Graph<T> class with DFS implementation)
public class Graph<T>
{
    private Dictionary<T, Node<T>> nodes;

    public Graph()
    {
        nodes = new Dictionary<T, Node<T>>();
    }

    public Node<T> AddNode(T value)
    {
        if (!nodes.ContainsKey(value))
        {
            Node<T> newNode = new Node<T>(value);
            nodes[value] = newNode;
            return newNode;
        }
        return nodes[value];
    }

    public Node<T> GetNode(T value)
    {
        if (nodes.TryGetValue(value, out Node<T> node))
        {
            return node;
        }
        return null;
    }

    public bool AddEdge(T value1, T value2)
    {
        Node<T> node1 = GetNode(value1);
        Node<T> node2 = GetNode(value2);

        if (node1 != null && node2 != null)
        {
            node1.AddNeighbor(node2);
            node2.AddNeighbor(node1); // For undirected graph
            return true;
        }
        return false;
    }

    // Optional: Method to remove an undirected edge
    public bool RemoveEdge(T value1, T value2)
    {
        Node<T> node1 = GetNode(value1);
        Node<T> node2 = GetNode(value2);

        if (node1 != null && node2 != null)
        {
            bool removed1 = node1.RemoveNeighbor(node2);
            bool removed2 = node2.RemoveNeighbor(node1); // For undirected graph
            return removed1 && removed2; // Both ends must be removed
        }
        return false;
    }

    // --- Depth-First Search Implementation ---
    public List<T> DepthFirstTraversal(T startValue)
    {
        List<T> visitedValues = new List<T>();
        // Use a HashSet for efficient checking of visited nodes
        HashSet<Node<T>> visitedNodes = new HashSet<Node<T>>();
        // Use a Stack to manage nodes to visit (LIFO)
        Stack<Node<T>> stack = new Stack<Node<T>>();

        Node<T> startNode = GetNode(startValue);

        // Handle case where start node doesn't exist
        if (startNode == null)
        {
            Console.WriteLine($"Start node '{startValue}' not found in the graph.");
            return visitedValues; // Return empty list
        }

        // Push the starting node onto the stack
        stack.Push(startNode);

        // While the stack is not empty
        while (stack.Count > 0)
        {
            // Pop the next node from the stack
            Node<T> currentNode = stack.Pop();

            // If we have already visited this node, skip it
            if (visitedNodes.Contains(currentNode))
            {
                continue;
            }

            // If not visited, mark it as visited and process it
            visitedNodes.Add(currentNode);
            visitedValues.Add(currentNode.Value); // Add value to result list
            // Console.WriteLine($"Visited: {currentNode.Value}"); // Optional: Print as we visit

            // Push all unvisited neighbors onto the stack
            // We can push in any order, the stack's LIFO nature determines which neighbor branch is explored first
            // Reversing the neighbors list before pushing can sometimes make the traversal order more predictable
            // depending on how the neighbors were added. Let's push them as they are for simplicity.
            foreach (Node<T> neighbor in currentNode.Neighbors)
            {
                if (!visitedNodes.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }
            }
        }

        return visitedValues; // Return the list of visited node values in DFS order
    }
}