using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSession
{
    public string worldSectionId;
    public string spawnPointId;

    public PlayerStats PlayerStats { get; private set; }
    public GameProgression GameProgression { get; private set; }
    
    public List<BossFightState> BossFightStates { get; private set; } = new List<BossFightState>();
    
    public event Action<BossFightState> OnBossFightState;
    public event Action<BossFightState> OnBossFightStateRemoved;
    public event Action OnGameSessionResetTempData;
    
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
    
    public void AddBossFightState(BossFightState state)
    {
        if (state == null)
        {
            Debug.LogError("Cannot add null BossFightState.");
            return;
        }
        
        BossFightStates.Add(state);
        OnBossFightState?.Invoke(state);
    }

    public void RemoveBossFightState(BossFightState state)
    {
        if (state == null)
        {
            Debug.LogError("Cannot remove null BossFightState.");
            return;
        }
        
        if (BossFightStates.Contains(state))
        {
            BossFightStates.Remove(state);
            OnBossFightStateRemoved?.Invoke(state);
        }
        else
        {
            Debug.LogWarning("BossFightState not found in the list.");
        }
    }
    
    public void ResetTemporaryData()
    {
        BossFightStates.Clear();
        OnGameSessionResetTempData?.Invoke();
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