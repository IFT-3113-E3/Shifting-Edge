using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [System.Serializable]
    public class EnemyConfig
    {
        public string enemyID;  // "Goblin", "Orc", etc.
        public float maxHealth;
    }

    [SerializeField] private List<EnemyConfig> configs;
    private Dictionary<string, float> _healthDatabase;

    private void Awake()
    {
        Instance = this;
        _healthDatabase = new Dictionary<string, float>();
        foreach (var config in configs)
        {
            _healthDatabase[config.enemyID] = config.maxHealth;
        }
    }

    public float GetHealthForEnemy(string enemyID)
    {
        return _healthDatabase.TryGetValue(enemyID, out float health) ? health : 100f; // Valeur par défaut
    }
}