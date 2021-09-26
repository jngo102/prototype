using UnityEngine;

public class LevelTransitionMessage
{
    public const string Name = "On" + nameof(LevelTransitionMessage);

    public string Level { get; }
    public string Gate { get; }

    public Vector2 GateOffset { get; }
    public Vector2 GateSize { get; }

    public LevelTransitionMessage(string level, string gate, Vector2 offset, Vector2 size)
    {
        Level = level;
        Gate = gate;
        GateOffset = offset;
        GateSize = size;
    }
}