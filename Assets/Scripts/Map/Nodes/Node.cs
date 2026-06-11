using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node cameFrom;
    public List<Node> neighbours;
    public float weight = 1f;

    public float gScore; // cost from start node to current node
    public float hScore; // estimated cost from the current node to the target node
    public float FScore() // used to determine which node to explore next in the A* algorithm
    {
        return gScore + hScore;
    }

    private void OnEnable()
    {
        AStarManager.instance.RegisterNode(this);
    }

    private void OnDisable()
    {
        AStarManager.instance.UnregisterNode(this);
    }
}
