using UnityEngine;

public class RestoreMessage
{
    public const string Name = "On" + nameof(RestoreMessage);

    public Vector2 Position { get; }

    public RestoreMessage(Vector2 position)
    {
        Position = position;
    }
}