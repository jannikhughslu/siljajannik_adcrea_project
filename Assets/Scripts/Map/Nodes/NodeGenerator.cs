using System.Collections.Generic;
using UnityEngine;

public class NodeGenerator : MonoBehaviour
{

    public Node nodeprefab;
    public List<Node> nodeList;

    public Player_Controller player;
    
    // Instanciate a node prefab for every floor tile and add it to a list of nodes.
    public void CreateNodes(GridMap[,] grid)
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] == GridMap.FLOOR)
                {
                    Node newNode = Instantiate(nodeprefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                    nodeList.Add(newNode);
                }
            }
        }
        CreateConnections();
    }

    // loop through list of nodes and call ConnectNodes() if they are next to each other
    void CreateConnections()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int j = i + 1; j < nodeList.Count; j++)
            {
                // if the distance between two nodes is smaller than or equal to 1, connect them both ways
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position) <= 1.5f)
                {
                    ConnectNodes(nodeList[i], nodeList[j]);
                    ConnectNodes(nodeList[j], nodeList[i]);
                }
            }
        }
        // SpawnPlayerAtCenter();
    }

    // connect two nodes by adding the target node to the neighbours list of the from node
    void ConnectNodes(Node from, Node to)
    {
        if (from == to) { return; }

        from.neighbours.Add(to);
    }


    

    // Move the existing player to the center node after the map is ready
    /*void SpawnPlayerAtCenter()
    {
        if (player == null || nodeList == null || nodeList.Count == 0)
        {
            return;
        }

        Vector2Int center = new Vector2Int(mapWidth / 2, mapHeight / 2);
        Node centerNode = null;

        for (int i = 0; i < nodeList.Count; i++)
        {
            Vector2 nodePosition = nodeList[i].transform.position;
            if ((int)nodePosition.x == center.x && (int)nodePosition.y == center.y)
            {
                centerNode = nodeList[i];
                break;
            }
        }

        if (centerNode == null)
        {
            centerNode = nodeList[0];
        }

        player.transform.position = centerNode.transform.position;
        player.currentNode = centerNode;
    }*/
}