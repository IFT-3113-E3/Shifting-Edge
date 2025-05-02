using UnityEngine;

public class GameSession
{
    public string worldSectionId;
    public string spawnPointId;

    public PlayerStats PlayerStats { get; private set; }
    public GameProgression GameProgression { get; private set; }
    
    public GameSession()
    {
        worldSectionId = string.Empty;
        spawnPointId = string.Empty;
        PlayerStats = ScriptableObject.CreateInstance<PlayerStats>();
        GameProgression = new GameProgression();
    }
    
    public GameSession(string worldSectionId, string spawnPointId, PlayerStats playerStats, GameProgression gameProgression)
    {
        this.worldSectionId = worldSectionId;
        this.spawnPointId = spawnPointId;
        PlayerStats = playerStats;
        GameProgression = gameProgression;
    }
    
    public void SaveData(ref SessionSaveData data)
    {
        data.worldSectionId = worldSectionId;
        data.spawnPointId = spawnPointId;
        
        if (PlayerStats != null)
        {
            PlayerStats.SaveData(ref data);
        }
        
        if (GameProgression != null)
        {
            GameProgression.SaveData(ref data);
        }
    }
    
    public void LoadData(SessionSaveData data)
    {
        worldSectionId = data.worldSectionId;
        spawnPointId = data.spawnPointId;
        
        if (PlayerStats != null)
        {
            PlayerStats.LoadData(data);
        }
        
        if (GameProgression != null)
        {
            GameProgression.LoadData(data);
        }
    }
}