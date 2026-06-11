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
        List<Node> candidates = new List<Node>(); // list of considered nodes for the path.
        HashSet<Node> inCandidates = new HashSet<Node>(); // to check if candidate already exists -> faster

        // STEP 1.
        foreach (Node node in allNodes)
        {
            node.gScore = float.MaxValue; // set gScore to infinity
        }
        startNode.gScore = 0;
        startNode.hScore = Vector2.Distance(startNode.transform.position, targetNode.transform.position); // hScore estimated airdistance
        candidates.Add(startNode);
        inCandidates.Add(startNode);

        
        while(candidates.Count > 0)
        {
            // STEP 2
            int lowestFScore = 0;
            // find node with the lowest fScore in openSet and set lowestFScore to its index
            // to determines where the shortest path comes from
            for (int i = 1; i < candidates.Count; i++)
            {
                if (candidates[i].FScore() < candidates[lowestFScore].FScore())
                {
                    lowestFScore = i;
                }
            }

            Node currentNode = candidates[lowestFScore];
            candidates.Remove(currentNode);
            inCandidates.Remove(currentNode);

            // Step 4
            // if target node reached 
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
                return path; // return reversed optimal path
            }

            // STEP 3
            // check neighbours of current node and update their gScore and hScore if we have found a better path to them
            foreach(Node neighbour in currentNode.neighbours)
            {
                // heldGScore to check if path from current node is better than  previously known paths of neighbour.
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, neighbour.transform.position) * neighbour.weight;
                if (heldGScore < neighbour.gScore)
                {
                    neighbour.cameFrom = currentNode;
                    neighbour.gScore = heldGScore;
                    neighbour.hScore = Vector2.Distance(neighbour.transform.position, targetNode.transform.position);

                    // if the neighbour is not already considered
                    if (!inCandidates.Contains(neighbour))
                    {
                        candidates.Add(neighbour);
                        inCandidates.Add(neighbour);
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
