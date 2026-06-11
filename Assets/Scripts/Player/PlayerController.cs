using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public int facingDirection = 1; // 1 = facing right; -1 = facing left

    private Rigidbody2D playerRB;
    private Animator playerAnimator;
    private LineRenderer navLineRenderer;

    public Node currentNode;
    public Node targetNode;
    public List<Node> path = new List<Node>();
    public float nodeReachDistance = 0.5f;


    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        navLineRenderer = GetComponent<LineRenderer>();
    }
    

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // keys: a= -1 and d= 1
        float verticalInput = Input.GetAxis("Vertical"); // keys: s= -1 and w= 1

        if (horizontalInput > 0 && transform.localScale.x < 0 ||
        horizontalInput < 0 && transform.localScale.x > 0) // Flip player if input is right but facing left or input is left but facing right
        {
            FlipPlayer();
        }

        // animate if input =! 0 -> animate
        playerAnimator.SetFloat("horizontal", Mathf.Abs(horizontalInput));
        playerAnimator.SetFloat("vertical", Mathf.Abs(verticalInput));

        playerRB.linearVelocity = new Vector2(horizontalInput, verticalInput) * speed; // move player

        CreatePath();
    }

    void FlipPlayer()
    {
        facingDirection *= -1; // facingDirection positive <---> negative
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    public void SetPlayerNodes(Node center, Node target)
    {
        if (center == null)
        {
            Debug.LogWarning("Center node is null. Cannot set player position.");
            return;
        }
        transform.position = center.transform.position; // set player to center of map
        currentNode = center;
        targetNode = target;
    }

    public void CreatePath()
    {
        if (AStarManager.instance == null || targetNode == null)
        {
            return;
        }

        if (currentNode == null)
        {
            currentNode = FindClosestNode(); // initial node is closest node to player
        }

        Node nearestNeighbour = FindClosestNeighbour(currentNode.neighbours);
        if (nearestNeighbour != null && nearestNeighbour != currentNode)
        {
            float distToNearestNode = Vector2.Distance(transform.position, nearestNeighbour.transform.position);
             float distToCurrent = Vector2.Distance(transform.position, currentNode.transform.position);
            if (distToNearestNode < distToCurrent)
            {
                currentNode = nearestNeighbour;

                if (currentNode == targetNode)
                {
                    Debug.Log("erreicht");
                }
            }
        }

        if (path.Count == 0 || path[0] != currentNode || path[path.Count - 1] != targetNode)
        {
            List<Node> newPath = AStarManager.instance.generatePath(currentNode, targetNode);
            if (newPath != null)
            {
                path = newPath;
                DrawPath(path);
            } 
        }
    }

    Node FindClosestNode()
    {
        Node closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (Node node in AStarManager.instance.AllNodes)
        {
            float currentDistance = Vector2.Distance(transform.position, node.transform.position);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestNode = node;
            }
        }
        return closestNode;
    }

    Node FindClosestNeighbour(List<Node> neighbours)
    {
        Node closest = null;
        float closestDist = float.MaxValue;
        foreach (Node neighbour in neighbours)
        {
            float dist = Vector2.Distance(transform.position, neighbour.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = neighbour; }
        }
        return closest;
    }


    void DrawPath(List<Node> path)
    {
        navLineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            navLineRenderer.SetPosition(i, path[i].transform.position);
        }
    }
}
