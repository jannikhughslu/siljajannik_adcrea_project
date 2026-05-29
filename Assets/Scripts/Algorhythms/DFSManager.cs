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

    // Returns the node that is farthest from the start node by hop count using iterative DFS.
    public Node FindFarthestNode(Node startNode)
    {
        if (startNode == null) return null;

        // Start values. startnode with depth of 1
        Node farthestNode = startNode;
        int maxDepth = 0;

        // Iterative DFS using a stack
        // Tupel of Node and its depth
        var stack = new Stack<(Node node, int depth)>();
        // HashSet to keep track of visited nodes
        var visited = new HashSet<Node>();

        // add startnode to stack with depth 0
        stack.Push((startNode, 0));

        while (stack.Count > 0)
        {
            // take upmost node from stack
            var (current, depth) = stack.Pop();

            // check if current node was already visited 
            // HashSet return false if current is already inside
            // continue to the next loop iteration if it is
            if (!visited.Add(current)) continue;

            // if current node is farther than the farthest node found so far, 
            // update farthest node and max depth
            if (depth > maxDepth)
            {
                maxDepth = depth;
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
