using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class AStarManager : MonoBehaviour
{
    public static AStarManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of AStarManager found!");
            return;
        }
        // set the instance to this script so it can be accessed from other scripts
        instance = this;
    }

    // generates a path from the start node to the target node using the A* algorithm
    public List<Node> generatePath(Node startNode, Node targetNode)
    {
        // openSet is a list of nodes that are being considered for the path.
        List<Node> openSet = new List<Node>();
        foreach (Node node in FindObjectsByType<Node>(FindObjectsSortMode.None))
        {
            // set the gScore of all nodes to infinity, because we haven't explored any nodes yet, and we want to make sure that any node we do explore will have a lower gScore than infinity
            node.gScore = float.MaxValue;
        }
        startNode.gScore = 0;
        // set the hScore of the start node to the distance from the start node to the target node, so it will be considered first in the openSet
        startNode.hScore = Vector2.Distance(startNode.transform.position, targetNode.transform.position);
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
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

            // check neighbours of current node and update their gScore and hScore if we have found a better path to them
            foreach(Node neighbour in currentNode.neighbours)
            {
                // heldGScore is used to check if the path from the start node to the neighbour through the current node is better than any previously known path to the neighbour.
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, neighbour.transform.position);
                if (heldGScore < neighbour.gScore)
                {
                    neighbour.cameFrom = currentNode;
                    neighbour.gScore = heldGScore;
                    neighbour.hScore = Vector2.Distance(neighbour.transform.position, targetNode.transform.position);

                    // if the neighbour is not already in the openSet, add it to the openSet so it will be considered for the path
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        // return null if there is no path from the start node to the target node
        return null;
        
    }
}
