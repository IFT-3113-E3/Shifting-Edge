using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public string enemyId;
    public string displayName;
    public float maxHealth = 100f;
    public int xpReward = 10; // XP donnée quand l'ennemi est vaincu

    // Vous pouvez ajouter d'autres attributs ici:
    // public float moveSpeed = 3f;
    // public float attackDamage = 10f;
    // public float attackRange = 1.5f;
    // etc.
}

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField]
    private List<EnemyData> enemyDatabase = new();

    private readonly Dictionary<string, EnemyData> enemyLookup = new();

    private void Awake()
    {
        // Pattern Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Construire le dictionnaire pour des recherches rapides
        foreach (var enemyData in enemyDatabase)
        {
            enemyLookup[enemyData.enemyId] = enemyData;
        }
    }

    public EnemyData GetEnemyData(string enemyId)
    {
        if (enemyLookup.TryGetValue(enemyId, out EnemyData data))
        {
            return data;
        }

        Debug.LogWarning($"Enemy ID '{enemyId}' not found in database!");
        return null;
    }

    // Pour l'inspecteur Unity, permet d'ajouter facilement un nouvel ennemi
    public void AddEnemyData(EnemyData data)
    {
        enemyDatabase.Add(data);
        enemyLookup[data.enemyId] = data;
    }
}
