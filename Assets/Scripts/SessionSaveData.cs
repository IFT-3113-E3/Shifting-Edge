using System;
using System.Collections.Generic;

[Serializable]
public class SessionSaveData
{
    public string worldSectionId;
    public string spawnPointId;

    public int currentHealth;
    public int maxHealth;
    public int maxAbilityStacks;
    public int abilityStacks;
    
    public List<string> collectibleIds = new List<string>();
    public List<string> defeatedBossIds = new List<string>();
}