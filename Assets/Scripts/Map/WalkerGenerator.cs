using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

// FSM to determine if a tile is a floor, Forrest, Rock, Bush or empty
public enum GridMap
{
    FLOOR,
    EMPTY,
    BUSH,
    ROCK,
    FOREST
}

// This class generates a random floor layout using a random walker algorithm
// Multiple walkers are created and move around the grid, creating floor tiles as they go. 
// The walkers have a chance to change direction, additionally create new walkers, or be removed. 
// The algorithm continues until a certain percentage of the grid is filled with floor tiles.
public class WalkerGenerator : MonoBehaviour
{


    public GridMap[,] gridHandler;
    public List<WalkerObject> walkers;
    public Tilemap tilemap;
    [SerializeField]
    public Tile[] tiles;

    // terrain Prefab
    public GameObject bushPrefab;
    public GameObject[] trees;
    public GameObject[] rocks;

    private int terrainElevation = 2;

    public int mapWidth = 30;
    public int mapHeight = 30;

    public int maxWalkers = 10;
    public int tileCount = default;
    public int terrainTileCount = default;
    public float fillPercent = 0.4f;
    private float terrainPercent = 0.3f;
    public float waitTime = 0.0f;

    public Node nodeprefab;
    public List<Node> nodeList;

    public Player_Controller player;

    private bool canDrawGizmos;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InizializeGrid();

    }

    void InizializeGrid()
    {
        // set grid dimensions
        gridHandler = new GridMap[mapWidth, mapHeight];
        // create list of walkers
        walkers = new List<WalkerObject>();

        // loop through grid and set all values to empty
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = GridMap.EMPTY;
                tilemap.SetTile(new Vector3Int(x, y, -2), tiles[0]);
            }
        }

        // get center of tilemap
        Vector3Int tileCenter = new Vector3Int(mapWidth / 2, mapHeight / 2, 0);

        // create first walker and set it to center of tilemap
        WalkerObject currWalker = new WalkerObject(new Vector2(tileCenter.x, tileCenter.y), GetDirection(), 0.5f, GridMap.FLOOR);
        gridHandler[tileCenter.x, tileCenter.y] = GridMap.FLOOR;
        tilemap.SetTile(tileCenter, tiles[1]);
        walkers.Add(currWalker);

        tileCount++;

        StartCoroutine(CreateFloors());
    }

    Vector2 GetDirection()
    {
        // generiert Zufallszahl 0-1 multipliert das mit 3.99
        // und FloorToInt runded es auf die nächste kleinere Zahl ab.
        // so entstehen 4 mögliche Werte: 0, 1, 2, 3
        int rand = Mathf.FloorToInt(Random.value * 3.99f);
        // int rand = Random.Range(0, 4);


        switch (rand)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            case 3:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    IEnumerator CreateFloors()
    {
        // compare tile count in the total size of grid to the fill percentage
        // loop until desired fill percentage is reached
        while ((float)tileCount / (float)(gridHandler.GetLength(0) * gridHandler.GetLength(1)) < fillPercent)
        {
            bool hasCreatedFloor = false;

            foreach (WalkerObject curWalker in walkers)
            {
                // get current position of walker and check if its not a floor tile
                Vector3Int curPos = new Vector3Int((int)curWalker.position.x, (int)curWalker.position.y, 0);
                if (gridHandler[curPos.x, curPos.y] != GridMap.FLOOR)
                {
                    tilemap.SetTile(curPos, tiles[1]);
                    tileCount++;
                    gridHandler[curPos.x, curPos.y] = GridMap.FLOOR;
                    hasCreatedFloor = true;
                }
            }

            // call chances methods for random walker algorithm and update position of them
            ChanceToRemove();
            ChanceToChangeDir();
            ChanceToCreate();
            UpdatePosition();

            if (hasCreatedFloor)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        CreateWalls();
    }


    // if random value is smaller than chance to change and there is more than 1 walker, remove the walker
    void ChanceToRemove()
    {
        int updateCount = walkers.Count;
        for (int i = 0; i < updateCount; i++)
        {
            if (Random.value < walkers[i].ChanceToChange && walkers.Count > 1)
            {
                walkers.RemoveAt(i);
                break;
            }
        }
    }

    // if random value is smaller than chance to change, change the direction of the walker
    void ChanceToChangeDir()
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            if (Random.value < walkers[i].ChanceToChange)
            {
                WalkerObject curWalker = walkers[i];
                curWalker.direction = GetDirection();
                walkers[i] = curWalker;
            }
        }
    }

    // if random value is smaller than chance to change and there are fewer walkers than the maximum, create a new walker
    void ChanceToCreate()
    {
        int updateCount = walkers.Count;
        for (int i = 0; i < updateCount; i++)
        {
            if (Random.value < walkers[i].ChanceToChange && walkers.Count < maxWalkers)
            {
                Vector2 newDir = GetDirection();
                Vector2 newPos = walkers[i].position;

                WalkerObject newWalker = new WalkerObject(newPos, newDir, 0.5f, GridMap.FLOOR);
                walkers.Add(newWalker);
            }
        }
    }

    // update the position of all walkers by adding the direction to the current position
    void UpdatePosition()
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            WalkerObject curWalker = walkers[i];

            curWalker.position += curWalker.direction;
            // clamp the position of the walker to the bounds of the grid, so it doesn't go out of bounds
            // Clamp(value, min, max) 
            curWalker.position.x = Mathf.Clamp(curWalker.position.x, 1, gridHandler.GetLength(0) - 2);
            curWalker.position.y = Mathf.Clamp(curWalker.position.y, 1, gridHandler.GetLength(1) - 2);
            walkers[i] = curWalker;
        }
    }

    // After floor layout is created, loop through grid and check if there is a floor tile. 
    // If there is, check if there are empty tiles next to it. 
    // If there are, set them to wall tiles.
    void CreateWalls()
    {
        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == GridMap.FLOOR)
                {

                    // Set Boarders, Bottom, Right, Left, Top
                    if (gridHandler[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[7]);
                        tilemap.SetTile(new Vector3Int(x, y, -1), tiles[Random.Range(16, 20)]);
                        //gridHandler[x, y - 1] = GridMap.WALL;
                    }
                    if (gridHandler[x + 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[5]);
                        tilemap.SetTile(new Vector3Int(x, y, -1), tiles[Random.Range(16, 20)]);

                        // gridHandler[x + 1, y] = GridMap.WALL;
                    }
                    if (gridHandler[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[9]);
                        tilemap.SetTile(new Vector3Int(x, y, -1), tiles[Random.Range(16, 20)]);
                        // gridHandler[x - 1, y] = GridMap.WALL;
                    }
                    if (gridHandler[x, y + 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[3]);
                        tilemap.SetTile(new Vector3Int(x, y, -1), tiles[Random.Range(16, 20)]);
                        // gridHandler[x, y + 1] = GridMap.WALL;
                    }

                    // set Boarders with cornerns
                    if (gridHandler[x, y - 1] == GridMap.EMPTY && gridHandler[x + 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[6]);
                    }
                    if (gridHandler[x, y - 1] == GridMap.EMPTY && gridHandler[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[8]);
                    }
                    if (gridHandler[x, y + 1] == GridMap.EMPTY && gridHandler[x + 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[4]);
                    }
                    if (gridHandler[x, y + 1] == GridMap.EMPTY && gridHandler[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[2]);
                        // gridHandler[x + 1, y] = GridMap.WALL;
                    }

                    // set Boarders with two sides
                    if (gridHandler[x, y - 1] == GridMap.EMPTY && gridHandler[x, y + 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[11]);
                    }
                    if (gridHandler[x + 1, y] == GridMap.EMPTY && gridHandler[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[10]);
                    }

                    // set Boarders with trhee sides
                    // left, right, top // path down
                    if (gridHandler[x - 1, y] == GridMap.EMPTY && gridHandler[x + 1, y] == GridMap.EMPTY && gridHandler[x, y + 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[12]);
                    }
                    // left, up, bottom
                    // path right
                    if (gridHandler[x - 1, y] == GridMap.EMPTY && gridHandler[x, y + 1] == GridMap.EMPTY && gridHandler[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[15]);
                    }
                    // top, bottom, right
                    // path left
                    if (gridHandler[x + 1, y] == GridMap.EMPTY && gridHandler[x, y + 1] == GridMap.EMPTY && gridHandler[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[13]);
                    }
                    // right, bottom, left
                    // path up
                    if (gridHandler[x - 1, y] == GridMap.EMPTY && gridHandler[x + 1, y] == GridMap.EMPTY && gridHandler[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tiles[14]);
                    }

                }
            }
        }
        CreateTerrain();
        
    }


    void PlaceTerrainSprite(GridMap terrainType, Vector3Int position)
    {
        if (terrainType == GridMap.BUSH)
        {
            Instantiate(bushPrefab, new Vector2(position.x + Random.Range(0f, 0.7f), position.y + Random.Range(0f, 0.7f)), Quaternion.identity);
        }
        if (terrainType == GridMap.ROCK)
        {
            Instantiate(rocks[Random.Range(0, 3)], new Vector2(position.x + Random.Range(0f, 0.7f), position.y + Random.Range(0f, 0.7f)), Quaternion.identity);
        }
        if (terrainType == GridMap.FOREST)
        {
            Instantiate(trees[Random.Range(0, 2)], new Vector2(position.x + Random.Range(0f, 0.7f), position.y + 1.4f), Quaternion.identity);
        }
    }

    void SpawnTerrainWalker(GridMap terrainType)
    {
        int numOfWalker = Random.Range(1, 4);
        int walkerCount = 0;


        //Creates a random number of Walkers. Min 2, Max 4
        // As long as the Random number of walker is not reached...
        while (walkerCount <= numOfWalker)
        {
            // get random position for a terrain walker
            Vector3Int pos = new Vector3Int(Random.Range(1, gridHandler.GetLength(0) - 1), Random.Range(1, gridHandler.GetLength(1) - 1), terrainElevation);

            // if position is on a Floor Tile create a walker
            if (gridHandler[pos.x, pos.y] == GridMap.FLOOR)
            {

                // create new terrain walker and add it to the list of terrain walkers
                WalkerObject newTerrainWalker = new WalkerObject(new Vector2(pos.x, pos.y), GetDirection(), 0.5f, terrainType);
                PlaceTerrainSprite(terrainType, pos);
                gridHandler[pos.x, pos.y] = terrainType;
                terrainTileCount++;

                walkers.Add(newTerrainWalker);
                walkerCount++;
            }
        }
    }

    void CreateTerrain()
    {
        walkers.Clear();
        //Randomly Spawnes a number of Walker between 2 an 4 for each Terrain Type (max. 12 Walkers)
        SpawnTerrainWalker(GridMap.BUSH);
        SpawnTerrainWalker(GridMap.FOREST);
        SpawnTerrainWalker(GridMap.ROCK);

        while ((float)terrainTileCount / (float)tileCount < (float)terrainPercent)
        {
            foreach (WalkerObject currTerrainWalker in walkers)
            {
                // get current position of walker and check if its not a floor tile
                Vector3Int curPos = new Vector3Int((int)currTerrainWalker.position.x, (int)currTerrainWalker.position.y, 0);
                if (gridHandler[curPos.x, curPos.y] == GridMap.FLOOR)
                {
                    PlaceTerrainSprite(currTerrainWalker.gridType, curPos);
                    terrainTileCount++;
                    gridHandler[curPos.x, curPos.y] = currTerrainWalker.gridType;
                }
            }
            ChanceToChangeDir();
            UpdatePosition();
        }
        CreateNodes();
    }

    // Instanciate a node prefab for every floor tile and add it to a list of nodes.
    void CreateNodes()
    {
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                if (gridHandler[x, y] == GridMap.FLOOR)
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
        canDrawGizmos = true;
        SpawnPlayerAtCenter();
    }

    // connect two nodes by adding the target node to the neighbours list of the from node
    void ConnectNodes(Node from, Node to)
    {
        if (from == to) { return; }

        from.neighbours.Add(to);
    }


    // Move the existing player to the center node after the map is ready
    void SpawnPlayerAtCenter()
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
    }

    // draw lines between connected nodes in the editor
    /*private void OnDrawGizmos()
    {
        if (canDrawGizmos)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < nodeList.Count; i++)
            {
                for (int j = 0; j < nodeList[i].neighbours.Count; j++)
                {
                    Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i].neighbours[j].transform.position);
                }
            }
        }
    }*/

}
