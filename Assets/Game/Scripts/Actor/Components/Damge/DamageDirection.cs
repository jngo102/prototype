using UnityEngine;

public enum DamageDirection
{
    Unknown = 0,

    Up,
    Right,
    Down,
    Left 
}

public static class DamageDirectionExtensions
{
    public static Vector2 GetVector(this DamageDirection direction)
    {
        switch (direction)
        {
            case DamageDirection.Up: return Vector2.up;
            case DamageDirection.Right: return Vector2.right;
            case DamageDirection.Down: return Vector2.down;
            case DamageDirection.Left: return Vector2.left;
        }

        return Vector2.zero;
    }
}