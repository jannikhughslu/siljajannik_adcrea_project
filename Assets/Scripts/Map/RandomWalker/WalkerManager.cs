using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerManager : MonoBehaviour
{
    public TilemapPainter tilemapPainter;
    public SpritePainter spritePainter;

    public int mapWidth = 30;
    public int mapHeight = 30;
    public int maxTileWalkers = 10;
    public float fillPercent = 0.4f;
    public float waitTime = 0.0f;
    private float terrainPercent = 0.3f;
    private int tileCount;
    private int terrainTileCount;

    private List<WalkerObject> walkers;
    public event System.Action OnMapGenerated;

    public GridMap[,] Grid { get; private set; }

    public void InitializeGrid()
    {
        Grid = new GridMap[mapWidth, mapHeight];
        walkers = new List<WalkerObject>();
        tileCount = 0;
        terrainTileCount = 0;

        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                Grid[x, y] = GridMap.EMPTY;
                tilemapPainter.SetTileById(new Vector3Int(x, y, -2), TileId.WATER_FULL);
            }
        }

        Vector3Int tileCenter = new Vector3Int(mapWidth / 2, mapHeight / 2, 0);
        WalkerObject currWalker = new WalkerObject(new Vector2(tileCenter.x, tileCenter.y), GetDirection(), 0.5f, GridMap.FLOOR);
        Grid[tileCenter.x, tileCenter.y] = GridMap.FLOOR;
        tilemapPainter.SetTileById(tileCenter, TileId.GRASS_FULL);
        walkers.Add(currWalker);
        tileCount++;

        StartCoroutine(CreateFloors());
    }

    IEnumerator CreateFloors()
    {
        while ((float)tileCount / (float)(Grid.GetLength(0) * Grid.GetLength(1)) < fillPercent)
        {
            bool hasCreatedFloor = false;

            foreach (WalkerObject curWalker in walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.position.x, (int)curWalker.position.y, 0);
                if (Grid[curPos.x, curPos.y] != GridMap.FLOOR)
                {
                    tilemapPainter.SetTileById(curPos, TileId.GRASS_FULL);
                    tileCount++;
                    Grid[curPos.x, curPos.y] = GridMap.FLOOR;
                    hasCreatedFloor = true;
                }
            }

            ChanceToRemove();
            ChanceToChangeDir();
            ChanceToCreate();
            UpdatePosition();

            if (hasCreatedFloor)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        tilemapPainter.CreateWalls(Grid);
        CreateTerrain();
    }

    void CreateTerrain()
    {
        walkers.Clear();

        SpawnTerrainWalkers(GridMap.BUSH);
        SpawnTerrainWalkers(GridMap.FOREST);
        SpawnTerrainWalkers(GridMap.ROCK);

        while ((float)terrainTileCount / (float)tileCount < terrainPercent)
        {
            foreach (WalkerObject currTerrainWalker in walkers)
            {
                Vector3Int curPos = new Vector3Int((int)currTerrainWalker.position.x, (int)currTerrainWalker.position.y, 0);
                if (Grid[curPos.x, curPos.y] == GridMap.FLOOR)
                {
                    spritePainter.PlaceSprite(curPos, currTerrainWalker.gridType);
                    terrainTileCount++;
                    Grid[curPos.x, curPos.y] = currTerrainWalker.gridType;
                }
            }
            ChanceToChangeDir();
            UpdatePosition();
        }
        OnMapGenerated?.Invoke();
    }

    void SpawnTerrainWalkers(GridMap terrainType)
    {
        int maxTypeWalker = Random.Range(2, 5);
        int walkerCount = 0;

        while (walkerCount < maxTypeWalker)
        {
            Vector3Int pos = new Vector3Int(
                Random.Range(1, Grid.GetLength(0) - 1),
                Random.Range(1, Grid.GetLength(1) - 1),
                0
            );

            if (Grid[pos.x, pos.y] == GridMap.FLOOR)
            {
                WalkerObject newTerrainWalker = new WalkerObject(new Vector2(pos.x, pos.y), GetDirection(), 0.5f, terrainType);
                walkers.Add(newTerrainWalker);

                Grid[pos.x, pos.y] = terrainType;
                spritePainter.PlaceSprite(pos, terrainType);
                
                terrainTileCount++;
                walkerCount++;
            }
        }
    }

    Vector2 GetDirection()
    {
        switch (Random.Range(0, 4))
        {
            case 0: return Vector2.down;
            case 1: return Vector2.left;
            case 2: return Vector2.up;
            case 3: return Vector2.right;
            default: return Vector2.zero;
        }
    }

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

    void ChanceToCreate()
    {
        int updateCount = walkers.Count;
        for (int i = 0; i < updateCount; i++)
        {
            if (Random.value < walkers[i].ChanceToChange && walkers.Count < maxTileWalkers)
            {
                WalkerObject newWalker = new WalkerObject(walkers[i].position, GetDirection(), 0.5f, GridMap.FLOOR);
                walkers.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            WalkerObject curWalker = walkers[i];
            curWalker.position += curWalker.direction;
            curWalker.position.x = Mathf.Clamp(curWalker.position.x, 1, Grid.GetLength(0) - 2);
            curWalker.position.y = Mathf.Clamp(curWalker.position.y, 1, Grid.GetLength(1) - 2);
            walkers[i] = curWalker;
        }
    }
}
