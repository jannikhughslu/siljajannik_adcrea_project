using UnityEngine;

public class SpritePainter : MonoBehaviour
{

    // terrain Prefab
    public GameObject bushPrefab;
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    public GameObject goldStonePrefab;
    

    public void PlaceSprite(Vector3Int position, GridMap terrainType)
    {
        // Randomness of position on tile
        float randomX = Random.Range(0f, 0.7f);
        float randomY = Random.Range(0f, 0.7f);

        if (terrainType == GridMap.BUSH)
        {
            Instantiate(bushPrefab, new Vector2(position.x + randomX, position.y + randomY), Quaternion.identity);
        }
        if (terrainType == GridMap.ROCK)
        {
            Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], new Vector2(position.x + randomX, position.y + randomY), Quaternion.identity);
        }
        if (terrainType == GridMap.FOREST)
        {
            Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], new Vector2(position.x + randomX, position.y + randomY), Quaternion.identity);
        }
    }

    public void SpawnGoldStone(Vector3 position)
    {
        Instantiate(goldStonePrefab, new Vector2(position.x, position.y), Quaternion.identity);
    }
}
