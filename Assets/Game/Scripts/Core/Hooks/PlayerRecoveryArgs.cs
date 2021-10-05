using UnityEngine;

public class PlayerRecoveryArgs
{
    public Vector2 Position { get; }

    public PlayerRecoveryArgs(Vector2 position)
    {
        Position = position;
    }
}