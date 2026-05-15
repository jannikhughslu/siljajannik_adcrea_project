using UnityEngine;

public class WalkerObject
{
    public Vector2 position;
    public Vector2 direction;
    public float ChanceToChange;
    public GridMap gridType;

    public WalkerObject(Vector2 pos, Vector2 dir, float chanceToChange, GridMap grType)
    {
        position = pos;
        direction = dir;
        ChanceToChange = chanceToChange;
        gridType = grType;
    }
}
