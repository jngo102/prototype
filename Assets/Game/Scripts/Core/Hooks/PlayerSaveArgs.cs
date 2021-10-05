using UnityEngine;

public class PlayerSaveArgs
{
    public string Entrance { get; }

    public PlayerSaveArgs(string entrance)
    {
        Entrance = entrance;
    }
}