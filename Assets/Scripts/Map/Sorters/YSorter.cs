using UnityEngine;

public class YSorter : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private int baseOrder = 2;   // Rock=1, Bush=2, Tree=3
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private int numberOfLayers = 4;
    [SerializeField] private int tilemapOffset = 3;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        int yOffset = Mathf.Max(0, mapHeight - Mathf.RoundToInt(sr.bounds.min.y));
        sr.sortingOrder = tilemapOffset + yOffset * numberOfLayers + baseOrder;
    }
}