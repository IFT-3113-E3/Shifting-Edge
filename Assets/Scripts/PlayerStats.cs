using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int maxHealth = 175;
    public int currentHealth = 175;
    public bool isDead = false;
    public int maxAbilityStacks = 3;
    public int abilityStacks;

    public event Action<int> OnHealthChanged;
    public event Action<int> OnAbilityStacksChanged;
    public event Action OnDeath;

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void SetMaxHealth(int health)
    {
        maxHealth = Mathf.Max(health, 1);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void SetAbilityStacks(int stacks)
    {
        abilityStacks = Mathf.Clamp(stacks, 0, maxAbilityStacks);
        OnAbilityStacksChanged?.Invoke(abilityStacks);
    }
    
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();
    }
    
    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void SaveData(ref SessionSaveData saveData)
    {
        saveData.currentHealth = currentHealth;
        saveData.maxHealth = maxHealth;
        saveData.maxAbilityStacks = maxAbilityStacks;
        saveData.abilityStacks = abilityStacks;
    }

    public void LoadData(SessionSaveData saveData)
    {
        currentHealth = saveData.currentHealth;
        maxHealth = saveData.maxHealth;
        maxAbilityStacks = saveData.maxAbilityStacks;
        abilityStacks = saveData.abilityStacks;
    }
}