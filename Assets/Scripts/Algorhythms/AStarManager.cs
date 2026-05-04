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
        instance = this;
    }

    public List<Node> generatePath(Node startNode, Node targetNode)
    {
        List<Node> openSet = new List<Node>();
        foreach (Node node in FindObjectsByType<Node>(FindObjectsSortMode.None))
        {
            node.gScore = float.MaxValue;
        }
        startNode.gScore = 0;
        startNode.hScore = Vector2.Distance(startNode.transform.position, targetNode.transform.position);
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            int lowestFScore = 0;
            
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FScore() < openSet[lowestFScore].FScore())
                {
                    lowestFScore = i;
                }
            }
            Node currentNode = openSet[lowestFScore];
            openSet.Remove(currentNode);

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

            foreach(Node neighbour in currentNode.neighbours)
            {
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, neighbour.transform.position);
                if (heldGScore < neighbour.gScore)
                {
                    neighbour.cameFrom = currentNode;
                    neighbour.gScore = heldGScore;
                    neighbour.hScore = Vector2.Distance(neighbour.transform.position, targetNode.transform.position);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }


        return null;
        
    }
}
