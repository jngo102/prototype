public class PlayerTransitArgs
{
    public string Level { get; }
    public string Entrance { get; }
    public EntranceInfo EntranceInfo { get; }

    public PlayerTransitArgs(string level, string entrance, EntranceInfo entranceInfo)
    {
        Level = level;
        Entrance = entrance;
        EntranceInfo = entranceInfo;
    }
}