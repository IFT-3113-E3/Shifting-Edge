public class GameSession
{
    public string worldSectionId;
    public string spawnPointId;

    public PlayerStats PlayerStats { get; private set; }
    
    public GameSession(string worldSectionId, string spawnPointId, PlayerStats playerStats)
    {
        this.worldSectionId = worldSectionId;
        this.spawnPointId = spawnPointId;
        PlayerStats = playerStats;
    }
}