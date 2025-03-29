using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyHealth : MonoBehaviour
{
    public float MaxHealth = 100f;
    public float CurrentHealth { get; private set; }
    
    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }
    
    public void TakeDamage(float amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        
        if (CurrentHealth <= 0)
        {
            // géré par EnemyAI
        }
    }
    
    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }
}