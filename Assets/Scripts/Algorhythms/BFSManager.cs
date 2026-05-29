using System.Collections.Generic;
using UnityEngine;

public class BFSManager : MonoBehaviour
{
    public static BFSManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of BFSManager found!");
            return;
        }
        instance = this;
    }

    // Returns the node that is farthest from the start node by shortest path (hop count) using BFS.
    public Node FindFarthestNode(Node startNode)
    {
        if (startNode == null) return null;

        Node farthestNode = startNode;

        var queue = new Queue<(Node node, int depth)>();
        var visited = new HashSet<Node>();

        queue.Enqueue((startNode, 0));
        visited.Add(startNode);

        int maxDepth = 0;

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            if (depth > maxDepth)
            {
                maxDepth = depth;
                farthestNode = current;
            }

            foreach (Node neighbour in current.neighbours)
            {
                if (visited.Add(neighbour))
                    queue.Enqueue((neighbour, depth + 1));
            }
        }

        return farthestNode;
    }
}
