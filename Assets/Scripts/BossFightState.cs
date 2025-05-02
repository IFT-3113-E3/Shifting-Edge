/// <summary>
/// Model object for active state of boss fights in the game. Contains information about the current boss fight, such as the boss type, health, and any special conditions.
/// </summary>
public class BossFightState
{
    public string BossType { get; private set; } // Type of the boss (e.g., "Dragon", "Goblin King")
    public int Health { get; private set; } // Current health of the boss
    public bool IsDefeated { get; private set; } // Indicates if the boss has been defeated
    public bool IsInProgress { get; private set; } // Indicates if the boss fight is currently in progress

    public BossFightState(string bossType, int health)
    {
        BossType = bossType;
        Health = health;
        IsDefeated = false;
        IsInProgress = true;
    }

    /// <summary>
    /// Reduces the boss's health by the specified amount.
    /// </summary>
    /// <param name="damage">The amount of damage to deal to the boss.</param>
    public void TakeDamage(int damage)
    {
        if (IsInProgress && !IsDefeated)
        {
            Health -= damage;
            if (Health <= 0)
            {
                IsDefeated = true;
                IsInProgress = false;
            }
        }
    }
}