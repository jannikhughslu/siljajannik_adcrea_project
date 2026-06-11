using System.Collections.Generic;
using UnityEngine;

public class DFSManager : MonoBehaviour
{
    public static DFSManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of DFSManager found!");
            return;
        }
        instance = this;
    }

    // Iterative DFS
    public Node FindFarthestNode(Node startNode)
    {
        if (startNode == null) return null;

        Node farthestNode = startNode;
        int farthestDepth = 0;
        
        Stack<(Node node, int depth)> stack = new Stack<(Node node, int depth)>(); // Tupel of Node and its depth
        HashSet<Node> visited = new HashSet<Node>();

        stack.Push((startNode, 0));

        while (stack.Count > 0)
        {
            (Node current, int depth) = stack.Pop();
            
            if (!visited.Add(current)) // Add return false if current is already inside
            {
                continue; // next iteration
            } 

            if (depth > farthestDepth)
            {
                farthestDepth = depth;
                farthestNode = current;
            }

            // add all unvisited neighbors of the current node to the stack with incremented depth
            foreach (Node neighbour in current.neighbours)
            {
                if (!visited.Contains(neighbour))
                    stack.Push((neighbour, depth + 1));
            }
        }
        return farthestNode;
    }
}
