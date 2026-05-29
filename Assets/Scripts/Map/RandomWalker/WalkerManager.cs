using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

// This class generates a random floor layout using a random walker algorithm
// Multiple walkers are created and move around the grid, creating floor tiles as they go. 
// The walkers have a chance to change direction, additionally create new walkers, or be removed. 
// The algorithm continues until a certain percentage of the grid is filled with floor tiles.
public class WalkerManager : MonoBehaviour
{
    // Walker fields
    public GridMap[,] gridHandler;
    public List<WalkerObject> walkers;

    // References to other classes
    public TilemapPainter tmPainter;
    public SpritePainter spPainter;
    public NodeGenerator nodeGenerator;

    public int mapWidth = 30;
    public int mapHeight = 30;

    public int maxWalkers = 10;
    public int tileCount = default;
    public int terrainTileCount = default;
    public float fillPercent = 0.4f;
    private float terrainPercent = 0.3f;
    public float waitTime = 0.0f;

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
                tmPainter.SetTileById(new Vector3Int(x, y, -2), TileId.WATER_FULL);
            }
        }

        // get center of tilemap
        Vector3Int tileCenter = new Vector3Int(mapWidth / 2, mapHeight / 2, 0);

        // create first walker and set it to center of tilemap
        WalkerObject currWalker = new WalkerObject(new Vector2(tileCenter.x, tileCenter.y), GetDirection(), 0.5f, GridMap.FLOOR);
        gridHandler[tileCenter.x, tileCenter.y] = GridMap.FLOOR;
        tmPainter.SetTileById(tileCenter, TileId.GRASS_FULL);
        walkers.Add(currWalker);

        tileCount++;

        StartCoroutine(CreateFloors());
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
                    tmPainter.SetTileById(curPos, TileId.GRASS_FULL);
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
        tmPainter.CreateWalls(gridHandler);
        CreateTerrain();
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
                    spPainter.PlaceTerrainSprite(curPos, currTerrainWalker.gridType);
                    terrainTileCount++;
                    gridHandler[curPos.x, curPos.y] = currTerrainWalker.gridType;
                }
            }
            ChanceToChangeDir();
            UpdatePosition();
        }
        nodeGenerator.CreateNodes(gridHandler);
        canDrawGizmos = true;
    }

     void SpawnTerrainWalker(GridMap terrainType)
    {
        int numOfWalker = Random.Range(2, 5);
        int walkerCount = 0;


        //Creates a random number of Walkers. Min 2, Max 4
        // As long as the Random number of walker is not reached...
        while (walkerCount <= numOfWalker)
        {
            // get random position for a terrain walker
            Vector3Int pos = new Vector3Int(Random.Range(1, gridHandler.GetLength(0) - 1), Random.Range(1, gridHandler.GetLength(1) - 1), 0);

            // if position is on a Floor Tile create a walker
            if (gridHandler[pos.x, pos.y] == GridMap.FLOOR)
            {

                // create new terrain walker and add it to the list of terrain walkers
                WalkerObject newTerrainWalker = new WalkerObject(new Vector2(pos.x, pos.y), GetDirection(), 0.5f, terrainType);
                spPainter.PlaceTerrainSprite(pos, terrainType);
                gridHandler[pos.x, pos.y] = terrainType;
                terrainTileCount++;

                walkers.Add(newTerrainWalker);
                walkerCount++;
            }
        }
    }



    Vector2 GetDirection()
    {
        // generiert eine Zahl zwischen 0 und 3
        int rand = Random.Range(0, 4);


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
