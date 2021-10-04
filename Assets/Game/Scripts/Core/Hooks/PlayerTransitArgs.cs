using UnityEngine;

public class PlayerTransitArgs
{
    public string Level { get; }
    public string Entrance { get; }

    public Vector2 GateOffset { get; }
    public Vector2 GateSize { get; }

    public PlayerTransitArgs(string level, string entrance, Vector2 offset, Vector2 size)
    {
        Level = level;
        Entrance = entrance;
        GateOffset = offset;
        GateSize = size;
    }
}