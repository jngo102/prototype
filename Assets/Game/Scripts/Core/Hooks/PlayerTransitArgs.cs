using UnityEngine;

public class PlayerTransitArgs
{
    public const string Name = "On" + nameof(PlayerTransitArgs);

    public string Level { get; }
    public string Gate { get; }

    public Vector2 GateOffset { get; }
    public Vector2 GateSize { get; }

    public PlayerTransitArgs(string level, string gate, Vector2 offset, Vector2 size)
    {
        Level = level;
        Gate = gate;
        GateOffset = offset;
        GateSize = size;
    }
}