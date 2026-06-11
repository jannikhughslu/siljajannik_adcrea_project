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
    public Node FindFarthestNode(Node startNode)
    {
        if (startNode == null) return null;

        Node farthestNode = startNode;

        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            farthestNode = queue.Dequeue(); // get next node in the queue

            foreach (Node neighbour in farthestNode.neighbours)
            {
                if (visited.Add(neighbour)) // Add returns false if the neighbour was already visited
                { 
                    queue.Enqueue(neighbour);
                }
            }
        }
        return farthestNode; // return last node processed
    }
}
