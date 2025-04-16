using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public string enemyId; // Nouvel identifiant qui sera utilisé pour récupérer les données
    public int minSpawnCount = 2;
    public int maxSpawnCount = 5;
    public float spawnHeight = 0.5f;
    [Range(0, 100)]
    public int spawnChance = 30; // Pourcentage de chance qu'un chunk génère des ennemis
}

public class EnemySpawn : MonoBehaviour
{
    public List<EnemySpawnData> enemyTypes = new();
    public float minDistanceFromPlayer = 15f;
    public float despawnDistance = 50f;

    private ChunkManager chunkManager;
    private Transform player;
    public GameObject healthBarPrefab;

    private readonly Dictionary<Vector2Int, List<GameObject>> spawnedEnemies = new();

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

        HashSet<Vector2Int> processedChunks = new();

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
        HashSet<Vector2Int> results = new();

        Vector3 playerPos = player.position;
        Vector2Int playerChunk = new(
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

    // Dans la méthode TrySpawnEnemiesInChunk, ajouter cette partie lorsque vous instanciez un ennemi:

    private void TrySpawnEnemiesInChunk(Vector2Int chunkPos)
    {
        foreach (var enemyType in enemyTypes)
        {
            if (Random.Range(0, 100) >= enemyType.spawnChance) continue;

            int spawnCount = Random.Range(enemyType.minSpawnCount, enemyType.maxSpawnCount + 1);
            List<GameObject> enemies = new();

            // Récupérer les données de l'ennemi depuis l'EnemyManager
            EnemyData enemyData = null;
            if (!string.IsNullOrEmpty(enemyType.enemyId))
            {
                enemyData = EnemyManager.Instance.GetEnemyData(enemyType.enemyId);
            }

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition = FindValidSpawnPoint(chunkPos);

                if (spawnPosition != Vector3.zero)
                {
                    GameObject enemy = Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);

                    // Appliquer les données de l'ennemi si disponibles
                    if (enemyData != null)
                    {
                        if (enemy.TryGetComponent<Status.EntityStatus>(out var entityStatus))
                        {
                            entityStatus.maxHealth = enemyData.maxHealth;
                            entityStatus.Revive(); // Reset health to new max value
                        }

                        // Possibilité d'attacher un composant pour afficher le nom
                        if (!enemy.TryGetComponent<EnemyIdentity>(out var identity))
                        {
                            identity = enemy.AddComponent<EnemyIdentity>();
                        }
                        identity.SetEnemyData(enemyData);

                        // Initialiser le composant XP
                        if (!enemy.TryGetComponent<EnemyXP>(out var enemyXP))
                        {
                            _ = enemy.AddComponent<EnemyXP>();
                        }
                        // La valeur d'XP sera initialisée dans le Start() de EnemyXP
                    }

                    enemies.Add(enemy);

                    if (enemy.TryGetComponent<EnemyAI>(out var enemyAI))
                    {
                        enemyAI.Initialize(player);
                    }
                }
            }

            if (enemies.Count > 0)
            {
                spawnedEnemies[chunkPos] = enemies;
                Debug.Log($"Spawned {enemies.Count} enemies in chunk {chunkPos}");
            }
        }
    }

    private Vector3 FindValidSpawnPoint(Vector2Int chunkPos)
    {
        // Code inchangé
        int maxAttempts = 10;
        int chunkSize = chunkManager.chunkSize;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Position aléatoire dans le chunk
            float x = (chunkPos.x * chunkSize) + Random.Range(0f, chunkSize);
            float z = (chunkPos.y * chunkSize) + Random.Range(0f, chunkSize);

            Vector3 testPoint = new(x, 100f, z); // Point de départ en hauteur

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
        // Code inchangé
        List<Vector2Int> chunksToRemove = new();

        foreach (var kvp in spawnedEnemies)
        {
            Vector2Int chunkPos = kvp.Key;
            List<GameObject> enemies = kvp.Value;

            // Vérifier si le chunk est hors de portée
            Vector3 chunkCenter = new(
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
