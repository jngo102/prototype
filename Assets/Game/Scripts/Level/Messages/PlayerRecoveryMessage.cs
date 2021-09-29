using UnityEngine;

public class PlayerRecoveryMessage
{
    public const string Name = "On" + nameof(PlayerRecoveryMessage);

    public Vector2 Position { get; }

    public PlayerRecoveryMessage(Vector2 position)
    {
        Position = position;
    }
}