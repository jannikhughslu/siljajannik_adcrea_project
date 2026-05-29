using UnityEngine;

public class SpritePainter : MonoBehaviour
{

    // terrain Prefab
    public GameObject bushPrefab;
    public GameObject[] trees;
    public GameObject[] rocks;
    public GameObject goldStonePrefab;
    

    public void PlaceTerrainSprite(Vector3Int position, GridMap terrainType)
    {
        float randomX = Random.Range(0f, 0.7f);
        float randomY = Random.Range(0f, 0.7f);

        if (terrainType == GridMap.BUSH)
        {
            Instantiate(bushPrefab, new Vector2(position.x + randomX, position.y + randomY), Quaternion.identity);
        }
        if (terrainType == GridMap.ROCK)
        {
            Instantiate(rocks[Random.Range(0, rocks.Length)], new Vector2(position.x + randomX, position.y + randomY), Quaternion.identity);
        }
        if (terrainType == GridMap.FOREST)
        {
            Instantiate(trees[Random.Range(0, trees.Length)], new Vector2(position.x + randomX, position.y + randomY), Quaternion.identity);
        }
    }

    public void SpawnGoldStone(Vector3 position)
    {
        Instantiate(goldStonePrefab, new Vector2(position.x, position.y), Quaternion.identity);
    }
}
