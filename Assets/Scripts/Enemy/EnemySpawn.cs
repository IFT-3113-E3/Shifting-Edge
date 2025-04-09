using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Status;

[System.Serializable]
public class EnemySpawnData
{
    public string enemyID;
    public GameObject enemyPrefab;
    public int minSpawnCount = 2;
    public int maxSpawnCount = 5;
    public float spawnHeight = 0.5f;
    [Range(0, 100)]
    public int spawnChance = 30;
}

public class EnemySpawn : MonoBehaviour
{
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    public float minDistanceFromPlayer = 15f;
    public float despawnDistance = 50f;
    
    private ChunkManager chunkManager;
    private Transform player;
    public GameObject healthBarPrefab;

    private Dictionary<Vector2Int, List<GameObject>> spawnedEnemies = new Dictionary<Vector2Int, List<GameObject>>();
    
    void Awake()
    {
        chunkManager = GetComponent<ChunkManager>();
        if (chunkManager == null)
        {
            Debug.LogError("EnemySpawner requires a ChunkManager component on the same GameObject.");
            enabled = false;
            return;
        }
    }
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found. Make sure the player has the 'Player' tag.");
            enabled = false;
            return;
        }
        
        StartCoroutine(CheckForNewChunks());
    }
    
    IEnumerator CheckForNewChunks()
    {
        // Attendre que la NavMesh soit générée
        yield return new WaitForSeconds(1f);
        
        HashSet<Vector2Int> processedChunks = new HashSet<Vector2Int>();
        
        while (true)
        {
            foreach (var chunk in GetLoadedChunks())
            {
                if (!processedChunks.Contains(chunk) && !spawnedEnemies.ContainsKey(chunk))
                {
                    yield return new WaitForSeconds(0.5f);
                    TrySpawnEnemiesInChunk(chunk);
                    processedChunks.Add(chunk);
                }
            }
            
            CheckForDespawn();            
            yield return new WaitForSeconds(1f);
        }
    }
    
    private HashSet<Vector2Int> GetLoadedChunks()
    {
        HashSet<Vector2Int> results = new HashSet<Vector2Int>();
        
        Vector3 playerPos = player.position;
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkManager.chunkSize),
            Mathf.FloorToInt(playerPos.z / chunkManager.chunkSize)
        );
        
        for (int z = -chunkManager.renderDistance; z <= chunkManager.renderDistance; z++)
        {
            for (int x = -chunkManager.renderDistance; x <= chunkManager.renderDistance; x++)
            {
                Vector2Int chunkPos = playerChunk + new Vector2Int(x, z);
                results.Add(chunkPos);
            }
        }
        
        return results;
    }
    
    private void TrySpawnEnemiesInChunk(Vector2Int chunkPos)
    {
        List<GameObject> chunkEnemies = new List<GameObject>();
        
        foreach (var enemyType in enemyTypes)
        {
            if (Random.Range(0, 100) >= enemyType.spawnChance) continue;

            int spawnCount = Random.Range(enemyType.minSpawnCount, enemyType.maxSpawnCount + 1);
            
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPos = FindValidSpawnPoint(chunkPos);
                if (spawnPos != Vector3.zero)
                {
                    GameObject enemy = Instantiate(enemyType.enemyPrefab, spawnPos, Quaternion.identity);
                    
                    // Configuration des HP via EnemyManager
                    if (EnemyManager.Instance != null)
                    {
                        float maxHealth = EnemyManager.Instance.GetHealthForEnemy(enemyType.enemyID);
                        EntityStatus status = enemy.GetComponent<EntityStatus>();
                        if (status != null)
                        {
                            status.maxHealth = maxHealth;
                            status.Revive();
                        }
                        else
                        {
                            Debug.LogWarning($"Enemy {enemyType.enemyID} doesn't have EntityStatus component!");
                        }
                    }
                    else
                    {
                        Debug.LogError("EnemyManager.Instance is null. Make sure it's properly initialized.");
                    }

                    chunkEnemies.Add(enemy);
                }
            }
        }

        if (chunkEnemies.Count > 0)
        {
            spawnedEnemies[chunkPos] = chunkEnemies;
        }
    }
    
    private Vector3 FindValidSpawnPoint(Vector2Int chunkPos)
    {
        int maxAttempts = 10;
        int chunkSize = chunkManager.chunkSize;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Position aléatoire dans le chunk
            float x = (chunkPos.x * chunkSize) + Random.Range(0f, chunkSize);
            float z = (chunkPos.y * chunkSize) + Random.Range(0f, chunkSize);
            
            Vector3 testPoint = new Vector3(x, 100f, z); // Point de départ en hauteur
            
            if (Physics.Raycast(testPoint, Vector3.down, out RaycastHit hit, 200f))
            {
                Vector3 spawnPoint = hit.point + Vector3.up * 0.5f; // Légèrement au-dessus du sol
                
                if (Vector3.Distance(spawnPoint, player.position) < minDistanceFromPlayer)
                {
                    continue; // si trop proche, essayer un autre point
                }
                
                // Vérifier si ce point est sur une NavMesh valide
                if (NavMesh.SamplePosition(spawnPoint, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    return navHit.position;
                }
            }
        }
        
        return Vector3.zero; // Pas de position valide trouvée
    }
    
    private void CheckForDespawn()
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        
        foreach (var kvp in spawnedEnemies)
        {
            Vector2Int chunkPos = kvp.Key;
            List<GameObject> enemies = kvp.Value;
            
            // Vérifier si le chunk est hors de portée
            Vector3 chunkCenter = new Vector3(
                (chunkPos.x * chunkManager.chunkSize) + (chunkManager.chunkSize / 2f),
                0,
                (chunkPos.y * chunkManager.chunkSize) + (chunkManager.chunkSize / 2f)
            );
            
            float distanceToPlayer = Vector3.Distance(new Vector3(chunkCenter.x, player.position.y, chunkCenter.z), player.position);
            
            if (distanceToPlayer > despawnDistance)
            {
                // Supprimer tous les ennemis de ce chunk
                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        Destroy(enemy);
                    }
                }
                
                chunksToRemove.Add(chunkPos);
            }
        }
        
        // Nettoyer les entrées
        foreach (var chunk in chunksToRemove)
        {
            spawnedEnemies.Remove(chunk);
        }
    }
}