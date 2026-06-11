using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TileEntry
{
    public TileId id;
    public Tile tile;
}

public class TilemapPainter : MonoBehaviour
{
    public Tilemap tilemap;
    [SerializeField] private TileEntry[] tileEntries;
    private Dictionary<TileId, Tile> tilesById;

    private void Awake()
    {
        tilesById = new Dictionary<TileId, Tile>();

        foreach (TileEntry entry in tileEntries)
        {
            tilesById[entry.id] = entry.tile;
        }
    }

     public Tile GetTileById(TileId id)
    {
        if (tilesById.TryGetValue(id, out Tile tile))
        {
            return tile;
        }

        Debug.LogError($"Tile not found: {id}");
        return null;
    }

    public Tile GetRandomFoam()
    {
        TileId[] Foams =
        {
            TileId.FOAM_1,
            TileId.FOAM_2,
            TileId.FOAM_3,
            TileId.FOAM_4
        };

        int randomIndex = Random.Range(0, Foams.Length);
        TileId randomId = Foams[randomIndex];

        return tilesById[randomId];
    }

    public void SetTileById(Vector3Int position, TileId id)
    {
        if (tilesById.TryGetValue(id, out Tile tile))
        {
            tilemap.SetTile(position, tile);
        }
        else
        {
            Debug.LogError($"Tile not found: {id}");
        }
    }

    public void CreateWalls(GridMap[,] grid)
    {
        for (int x = 0; x < grid.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < grid.GetLength(1) - 1; y++)
            {
                if (grid[x, y] == GridMap.FLOOR)
                {

                    // Set Boarders, Bottom, Right, Left, Top
                    if (grid[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_B));
                        tilemap.SetTile(new Vector3Int(x, y, -1), GetRandomFoam());
                    }
                    if (grid[x + 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_R));
                        tilemap.SetTile(new Vector3Int(x, y, -1), GetRandomFoam());
                    }
                    if (grid[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_L));
                        tilemap.SetTile(new Vector3Int(x, y, -1), GetRandomFoam());
                    }
                    if (grid[x, y + 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_T));
                        tilemap.SetTile(new Vector3Int(x, y, -1), GetRandomFoam());
                    }

                    // set Boarders with cornerns
                    if (grid[x, y - 1] == GridMap.EMPTY && grid[x + 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_RB));
                    }
                    if (grid[x, y - 1] == GridMap.EMPTY && grid[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_LB));
                    }
                    if (grid[x, y + 1] == GridMap.EMPTY && grid[x + 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_RT));
                    }
                    if (grid[x, y + 1] == GridMap.EMPTY && grid[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_LT));
                        // gridHandler[x + 1, y] = GridMap.WALL;
                    }

                    // set Boarders with two sides
                    if (grid[x, y - 1] == GridMap.EMPTY && grid[x, y + 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_TB));
                    }
                    if (grid[x + 1, y] == GridMap.EMPTY && grid[x - 1, y] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_LR));
                    }

                    // set Boarders with trhee sides
                    // left, right, top // path down
                    if (grid[x - 1, y] == GridMap.EMPTY && grid[x + 1, y] == GridMap.EMPTY && grid[x, y + 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_LTR));
                    }
                    // left, up, bottom
                    // path right
                    if (grid[x - 1, y] == GridMap.EMPTY && grid[x, y + 1] == GridMap.EMPTY && grid[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_LTB));
                    }
                    // top, bottom, right
                    // path left
                    if (grid[x + 1, y] == GridMap.EMPTY && grid[x, y + 1] == GridMap.EMPTY && grid[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_TRB));
                    }
                    // right, bottom, left
                    // path up
                    if (grid[x - 1, y] == GridMap.EMPTY && grid[x + 1, y] == GridMap.EMPTY && grid[x, y - 1] == GridMap.EMPTY)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), GetTileById(TileId.GRASS_RBL));
                    }
                }
            }
        }
    }
}
