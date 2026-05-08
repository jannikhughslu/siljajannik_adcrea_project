using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// This class generates a random floor layout using a random walker algorithm
// Multiple walkers are created and move around the grid, creating floor tiles as they go. 
// The walkers have a chance to change direction, additionally create new walkers, or be removed. 
// The algorithm continues until a certain percentage of the grid is filled with floor tiles.
public class WalkerGenerator : MonoBehaviour
{
    // FSM to determine if a tile is a floor, wall or empty
    public enum Grid
    {
        FLOOR,
        WALL,
        EMPTY,
        TERRAIN
    }

    public Grid[,] gridHandler;

    public enum Terrain
    {
        BUSH,
        ROCK,
        FOREST
    }

    public Terrain terrainType;

    public List<WalkerObject> walkers;
    public List<TerrainObject> terrainWalkers;
    public Tilemap tilemap;
    public Tile floorTile;
    public Tile wallTileTop;
    public Tile wallTileLeft;
    public Tile wallTileRight;
    public Tile wallTileBottom;
    // terrain Tiles
    public Sprite TerrainSprite;

    public int mapWidth = 30;
    public int mapHeight = 30;

    public int maxWalkers = 10;
    public int tileCount = default;
    public float fillPercent = 0.4f;
    public float terrainPercent;
    public float waitTime = 0.05f;

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
        gridHandler = new Grid[mapWidth, mapHeight];
        // create list of walkers
        walkers = new List<WalkerObject>();

        // loop through grid and set all values to empty
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = Grid.EMPTY;
            }
        }

        // get center of tilemap
        Vector3Int tileCenter = new Vector3Int(mapWidth / 2, mapHeight / 2, 0);

        // create first walker and set it to center of tilemap
        WalkerObject currWalker = new WalkerObject(new Vector2(tileCenter.x, tileCenter.y), GetDirection(), 0.5f);
        gridHandler[tileCenter.x, tileCenter.y] = Grid.FLOOR;
        tilemap.SetTile(tileCenter, floorTile);
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
                if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                {
                    tilemap.SetTile(curPos, floorTile);
                    tileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
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
        StartCoroutine(CreateWalls());
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

                WalkerObject newWalker = new WalkerObject(newPos, newDir, 0.5f);
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
    IEnumerator CreateWalls()
    {
        for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
        {
            for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
            {
                if (gridHandler[x, y] == Grid.FLOOR)
                {
                    bool hasCreatedWall = false;

                    if (gridHandler[x, y - 1] == Grid.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y - 1, 0), wallTileBottom);
                        gridHandler[x, y - 1] = Grid.WALL;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x + 1, y] == Grid.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x + 1, y, 0), wallTileRight);
                        gridHandler[x + 1, y] = Grid.WALL;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x - 1, y] == Grid.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x - 1, y, 0), wallTileLeft);
                        gridHandler[x - 1, y] = Grid.WALL;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x, y + 1] == Grid.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y + 1, 0), wallTileTop);
                        gridHandler[x, y + 1] = Grid.WALL;
                        hasCreatedWall = true;
                    }
                    

                    if (hasCreatedWall)
                    {
                        yield return new WaitForSeconds(waitTime);
                    }
                }
            }
        }
        CreateNodes();
    }


    void PlaceTerrainSprites(Sprite terrainSprite)
    {

    }
    void TerrainWalker(Terrain terrainType, Sprite terrainSprite, float terrainPercent)
    {
        // get random position for terrain walker
        terrainStartX = Random.Range(1, gridHandler.GetLength(0) - 1);
        terrainStartY = Random.Range(1, gridHandler.GetLength(1) - 1);
        terrainElevation = 1f;
        // create walker for the terrain and set it to a random position on the tilemap
        TerrainObject terrainWalker = new TerrainObject(new Vector2(terrainStartX, terrainStartY), GetDirection(), 0.5f);
        gridHandler[terrainStartX, terrainStartY] = Terrain.terrainType;
        
        //Set the sprite of the terrain tile based on the terrain type
        GameObject terrainObject = new GameObject(Terrain.terrainType.ToString());
        // set the position of the terrain object to the position of the terrain walker
        terrainObject.transform.position = new Vector3Int(terrainStartX, terrainStartY, TerrainElevation);
        terrainObject.AddComponent<SpriteRenderer>().sprite = terrainSprite;
        
        terrainWalkers.Add(terrainWalker);
        
        tileCount++;

        // compare tile count in the total size of grid to the fill percentage
        // loop until desired fill percentage is reached
        while ((float)tileCount / (float)(gridHandler.GetLength(0) * gridHandler.GetLength(1)) < terrainPercent)
        {
            bool hasCreatedTerrain = false;

            foreach (TerrainObject curWalker in terrainWalkers)
            {
                // get current position of walker and check if its a floor tile
                Vector3Int curPos = new Vector3Int((int)curWalker.position.x, (int)curWalker.position.y, 0);
                if (gridHandler[curPos.x, curPos.y] == Grid.FLOOR)
                {
                    tilemap.SetTile(curPos, terrainTile);
                    tileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.TERRAIN;
                    hasCreatedTerrain = true;
                }
                // if tile is empty, move walker back to previous position and change direction
                if (gridHandler[curPos.x, curPos.y] == Grid.EMPTY)
                {
                    curPos.x = curPos.x -1;
                    curWalker.direction = GetDirection();
                }
            }
        }
    }
    
    void CreateTerrain()
    {
        switch (terrainType)
        {
            case Terrain.BUSH:
                StartCoroutine(CreateTerrain(terrainType, bushTile, terrainPercent));
                break;
            case Terrain.FOREST:
                StartCoroutine(CreateTerrain(terrainType, woodsTile, terrainPercent));
                break;
            case Terrain.ROCK:
                StartCoroutine(CreateTerrain(terrainType, rocksTile, terrainPercent));
                break;
        }
    }

    // Instanciate a node prefab for every floor tile and add it to a list of nodes.
    void CreateNodes()
    {
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                if (gridHandler[x, y] == Grid.FLOOR)
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
        for(int i = 0; i < nodeList.Count; i++)
        {
            for (int j = i+1; j < nodeList.Count; j++)
            {
                // if the distance between two nodes is smaller than or equal to 1, connect them both ways
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position) <= 1.0f)
                {
                    ConnectNodes(nodeList[i], nodeList[j]);
                    ConnectNodes(nodeList[j], nodeList[i]);
                }
            }
        }
        canDrawGizmos = true;
        SpawnPlayer();
    }

    // connect two nodes by adding the target node to the neighbours list of the from node
    void ConnectNodes(Node from, Node to)
    {
        if(from == to){return;}

        from.neighbours.Add(to);
    }


    // Spawn the player at a random node
    void SpawnPlayer()
    {
        Node randNode = nodeList[Random.Range(0, nodeList.Count)];

        Player_Controller newPlayer = Instantiate(player, randNode.transform.position, Quaternion.identity);

        newPlayer.currentNode = randNode;
        
    }

    // draw lines between connected nodes in the editor
    /*private void OnDrawGizmos()
    {
        if (canDrawGizmos)
        {
            Gizmos.color = Color.blue;
            for(int i = 0; i < nodeList.Count; i++)
            {
                for(int j = 0; j < nodeList[i].neighbours.Count; j++)
                {
                    Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i].neighbours[j].transform.position);
                }
            }
        }
    }*/



}
