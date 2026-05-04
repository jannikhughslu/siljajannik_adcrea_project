using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    public Node currentNode;
    public List<Node> path = new List<Node>();


    // Update is called once per frame
    void Update()
    {
        CreatePath();
    }

    public void CreatePath()
    {
        if (path.Count > 0)
        {
            int x = 0;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[x].transform.position.x, path[x].transform.position.y, -2), 5 * Time.deltaTime);

            if (Vector2.Distance(transform.position, path[x].transform.position) < 0.1f)
            {
                currentNode = path[x];
                path.RemoveAt(x);
            }
        }
        else
        {
            Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
            while(path == null || path.Count == 0)
            {
                path = AStarManager.instance.generatePath(currentNode, nodes[Random.Range(0, nodes.Length)]);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (path.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 1; i < path.Count; i++)
            {
                Gizmos.DrawLine(path[i].transform.position, path[i - 1].transform.position);
            }
        }
    }
}
