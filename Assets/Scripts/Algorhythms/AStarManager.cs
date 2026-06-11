using System.Collections.Generic;
using UnityEngine;

public class AStarManager : MonoBehaviour
{
    public static AStarManager instance;

    private HashSet<Node> allNodes = new HashSet<Node>();
    public HashSet<Node> AllNodes
    {
        get { return allNodes; }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of AStarManager found!");
            return;
        }
        instance = this;
    }

    public List<Node> generatePath(Node startNode, Node targetNode)
    {
        // openSet is a list of nodes that are being considered for the path.
        List<Node> openSet = new List<Node>();
        // hashset to check if openset contains node => faster
        HashSet<Node> inOpenSet = new HashSet<Node>();

        // STEP 1.
        foreach (Node node in allNodes)
        {
            node.gScore = float.MaxValue; // set gScore to infinity
        }
        startNode.gScore = 0;
        startNode.hScore = Vector2.Distance(startNode.transform.position, targetNode.transform.position); // hScore estimated airdistance
        openSet.Add(startNode);
        inOpenSet.Add(startNode);

        
        while(openSet.Count > 0)
        {
            // STEP 2
            int lowestFScore = 0;
            // find node with the lowest fScore in openSet and set lowestFScore to its index
            // Part of the algorithm that determines where the shortest path comes from
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FScore() < openSet[lowestFScore].FScore())
                {
                    lowestFScore = i;
                }
            }

            // currentNode is the one withe lowest current fScore
            Node currentNode = openSet[lowestFScore];
            openSet.Remove(currentNode);
            inOpenSet.Remove(currentNode);

            // Step 4
            // if we have reached the target node, 
            // return optimal path by following the cameFrom nodes from the target node back to the start node
            // reverse path so it goes from start node to target node
            if (currentNode == targetNode)
            {
                List<Node> path = new List<Node>();
                path.Insert(0, targetNode);

                while (currentNode != startNode)
                {
                    currentNode = currentNode.cameFrom;
                    path.Add(currentNode);
                    
                }
                path.Reverse();
                return path;
            }

            // STEP 3
            // check neighbours of current node and update their gScore and hScore if we have found a better path to them
            foreach(Node neighbour in currentNode.neighbours)
            {
                // heldGScore is used to check if the path from current node is better than any previously known path to the neighbour.
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, neighbour.transform.position) * neighbour.weight;
                if (heldGScore < neighbour.gScore)
                {
                    neighbour.cameFrom = currentNode;
                    neighbour.gScore = heldGScore;
                    neighbour.hScore = Vector2.Distance(neighbour.transform.position, targetNode.transform.position);

                    // if the neighbour is not already considered, add it
                    if (!inOpenSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        inOpenSet.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }


    public void RegisterNode(Node node)
    {
        allNodes.Add(node);
    }

    public void UnregisterNode(Node node)
    {
        allNodes.Remove(node);
    }

    
}
