using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public WalkerManager walkerManager;
    public NodeGenerator nodeGenerator;
    public SpritePainter spPainter;
    public PlayerController player;

    void Start()
    {
        walkerManager.OnMapGenerated += OnMapReady;
        walkerManager.InitializeGrid();
    }

    void OnDestroy()
    {
        walkerManager.OnMapGenerated -= OnMapReady;
    }

    void OnMapReady()
    {
        GridMap[,] grid = walkerManager.Grid;

        nodeGenerator.CreateNodes(grid);

        Node center = nodeGenerator.GetNodeAtCenter(grid);
        Node targetNode = BFSManager.instance.FindFarthestNode(center);

        spPainter.SpawnGoldStone(targetNode.transform.position);
        player.SetPlayerNodes(center, targetNode);
    }
}
