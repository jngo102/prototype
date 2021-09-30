using UnityEngine;

public class PlayerRecoveryArgs
{
    public const string Name = "On" + nameof(PlayerRecoveryArgs);

    public Vector2 Position { get; }

    public PlayerRecoveryArgs(Vector2 position)
    {
        Position = position;
    }
}