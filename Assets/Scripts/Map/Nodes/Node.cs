using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node cameFrom;
    public List<Node> neighbours;
    public float weight = 1f;

    // gScore is the cost from the start node to the current node
    // in other words: how many steps it took to get from the start node to the current node
    public float gScore;
    // hScore is the estimated cost from the current node to the target node
    // in other words: how many steps it would take to get from the current node to the target node if there were no obstacles in the way
    public float hScore;
    // FScore is the sum of gScore and hScore, and is used to determine which node to explore next in the A* algorithm
    public float FScore()
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
