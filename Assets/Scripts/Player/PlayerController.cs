using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
     // Movement speed of player character
    public float speed = 5.0f;
    // facing Direction = player scale X value. 1 = facingRight
    public int facingDirection = 1;
    private Rigidbody2D playerRB;
    private Animator playerAnimator;
    private LineRenderer navLineRenderer;

    // Node related variables
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
        // key input: a/< left: -1; d/> right: 1; no input: 0;
        float horizontalInput = Input.GetAxis("Horizontal");
        // key input: s/< down: -1; w/> up 1; no input: 0;
        float verticalInput = Input.GetAxis("Vertical");

        // Check if horizontal Input switches from left to right
        // Check input and see if it doesn't match current localScale.x (facingDirection)
        if (horizontalInput > 0 && transform.localScale.x < 0 ||
        horizontalInput < 0 && transform.localScale.x > 0)
        {
            FlipPlayer();
        }

        // Set input values to self definded variables "horizontal" and "vertical" in animator
        // Animator animates if input 0<. Input can sometimes be >0.
        // Mathf.Abs() turn number into absolute (positive) numbers.
        // Logic now: If input =! 0 -> animate
        playerAnimator.SetFloat("horizontal", Mathf.Abs(horizontalInput));
        playerAnimator.SetFloat("vertical", Mathf.Abs(verticalInput));

        // Move player
        playerRB.linearVelocity = new Vector2(horizontalInput, verticalInput) * speed;

        CreatePath();
    }

    void FlipPlayer()
    {
        // Switch facingDirection positive <---> negative
        facingDirection *= -1;
        // Set localScale.x to facingDirection. Keep .y and .z
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // Move the existing player to the center node after the map is ready
    public void SetPlayerNodes(Node center, Node target)
    {

        if (center == null)
        {
            Debug.LogWarning("Center node is null. Cannot set player position.");
            return;
        }
        transform.position = center.transform.position;
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
            // select initial node to be closest node to player
            currentNode = FindClosestNode();
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
