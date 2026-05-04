using UnityEngine;

public class WalkerObject
{
    public Vector2 position;
    public Vector2 direction;
    public float ChanceToChange;

    public WalkerObject(Vector2 pos, Vector2 dir, float chanceToChange)
    {
        position = pos;
        direction = dir;
        ChanceToChange = chanceToChange;
    }
}
