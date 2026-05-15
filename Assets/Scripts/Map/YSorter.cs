using UnityEngine;

public class YSorter : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private int baseOrder = 3;
    [SerializeField] private int mapHeight = 50;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        sr.sortingOrder =+ (mapHeight - Mathf.RoundToInt(transform.position.y));
    }
}