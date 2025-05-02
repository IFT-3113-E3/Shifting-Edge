using System;
using UnityEngine;

/// <summary>
/// Model object for active state of boss fights in the game. Contains information about the current boss fight, such as the boss type, health, and any special conditions.
/// </summary>
public class BossFightState
{
    public string BossType { get; private set; } // Type of the boss (e.g., "Dragon", "Goblin King")
    public int MaxHealth { get; private set; } // Maximum health of the boss
    public int Health { get; private set; } // Current health of the boss
    public bool IsDefeated { get; private set; } // Indicates if the boss has been defeated
    public bool IsInProgress { get; private set; } // Indicates if the boss fight is currently in progress

    public event Action<int> OnHealthChanged; // Event triggered when the boss's health changes
    public event Action OnDefeated; // Event triggered when the boss is defeated
    
    public BossFightState(string bossType, int health, int maxHealth)
    {
        BossType = bossType;
        MaxHealth = maxHealth;
        Health = health;
        IsDefeated = false;
        IsInProgress = true;
    }
    
    public void SetDefeated()
    {
        IsDefeated = true;
        IsInProgress = false;
        OnDefeated?.Invoke();
    }

    public void SetHealth(int health) 
    {
        Health = Mathf.Clamp(health, 0, MaxHealth);
        OnHealthChanged?.Invoke(Health);
    }
}