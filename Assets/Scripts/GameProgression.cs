using System;
using System.Collections.Generic;

/// <summary>
/// Model object for anything regarding game progression. This will be used to track the player's progress in the game.
/// </summary>
[Serializable]
public class GameProgression
{
    private readonly HashSet<string> _collectedCollectibles = new();
    
    private readonly HashSet<string> _defeatedBosses = new();
    public IReadOnlyCollection<string> DefeatedBosses => _defeatedBosses;

    public event Action<string> OnBossDefeated;
    public event Action<string> OnCollectibleCollected;
    
    public void MarkCollectibleCollected(string collectibleId)
    {
        if (_collectedCollectibles.Add(collectibleId))
            OnCollectibleCollected?.Invoke(collectibleId);
    }
    
    public bool HasCollected(string collectibleId) =>
        _collectedCollectibles.Contains(collectibleId);

    public void MarkBossDefeated(string bossId)
    {
        if (_defeatedBosses.Add(bossId))
            OnBossDefeated?.Invoke(bossId);
    }

    public bool HasDefeated(string bossId) =>
        _defeatedBosses.Contains(bossId);
    
    public void SaveData(ref SessionSaveData saveData)
    {
        saveData.collectibleIds = new List<string>(_collectedCollectibles);
        saveData.defeatedBossIds = new List<string>(_defeatedBosses);
    }
    
    public void LoadData(SessionSaveData saveData)
    {
        _collectedCollectibles.Clear();
        if (saveData.collectibleIds != null)
        {
            foreach (var collectibleId in saveData.collectibleIds)
            {
                _collectedCollectibles.Add(collectibleId);
            }
        }
        
        _defeatedBosses.Clear();
        if (saveData.defeatedBossIds != null)
        {
            foreach (var bossId in saveData.defeatedBossIds)
            {
                _defeatedBosses.Add(bossId);
            }
        }
    }
}