using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Status;


public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public string enemyId;
        public GameObject enemyPrefab;
        public int minSpawnCount = 2;
        public int maxSpawnCount = 5;
        public float spawnHeight = 0.5f;
        [Range(0, 100)]
        public int spawnChance = 100;
    }
    [Header("Spawn Settings")]
    public List<EnemySpawnData> enemyTypes = new()
    {
        new EnemySpawnData { enemyId = "1", enemyPrefab = null, minSpawnCount = 3, maxSpawnCount = 5, spawnChance = 100 },
        new EnemySpawnData { enemyId = "2", enemyPrefab = null, minSpawnCount = 4, maxSpawnCount = 5, spawnChance = 100 },
    };
    public Terrain targetTerrain;
    [Range(0f, 60f)]
    public float maxSlopeAngle = 20f;

    [Header("Spawn Frequency")]
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 15f;
    public int maxEnemiesOnTerrain = 50;

    [Header("Distance Parameters")]
    public float minDistanceFromPlayer = 15f;
    public float maxDistanceFromPlayer = 5000f;
    public float despawnDistance = 5000f;

    [Header("References")]
    public GameObject healthBarPrefab;

    private Transform player;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private int currentEnemyCount = 0;

    void Awake()
    {
        if (targetTerrain == null)
        {
            targetTerrain = FindObjectOfType<Terrain>();
            if (targetTerrain == null)
            {
                Debug.LogError("No terrain found! Please assign a terrain to the EnemySpawner.");
                enabled = false;
                return;
            }
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

        // Start spawning enemies
        StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        yield return new WaitForSeconds(1f); // Wait for NavMesh to be fully generated

        while (true)
        {
            // Check if player exists
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player")?.transform;
                if (player == null)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
            }

            // Remove null references (destroyed enemies)
            spawnedEnemies.RemoveAll(enemy => enemy == null);
            currentEnemyCount = spawnedEnemies.Count;

            // Only spawn more enemies if we're below the maximum
            if (currentEnemyCount < maxEnemiesOnTerrain)
            {
                Debug.Log($"Current enemy count: {currentEnemyCount}, Max allowed: {maxEnemiesOnTerrain}");
                Debug.Log($"Spawning enemies...");
                Debug.Log($"Enemy types available: {enemyTypes.Count}");
                // Spawn each type of enemy
                foreach (var enemyType in enemyTypes)
                {
                    Debug.Log($"Checking spawn chance for {enemyType.enemyId}");
                    if (Random.Range(0, 100) >= enemyType.spawnChance) continue;

                    int spawnCount = Random.Range(enemyType.minSpawnCount, enemyType.maxSpawnCount + 1);

                    // Limit spawn count to keep under the maximum
                    spawnCount = Mathf.Min(spawnCount, maxEnemiesOnTerrain - currentEnemyCount);

                    // Skip if we can't spawn any more
                    if (spawnCount <= 0) continue;

                    yield return StartCoroutine(SpawnEnemiesOfType(enemyType, spawnCount));
                }
            }

            // Check for enemies to despawn
            CheckForDespawn();

            Debug.Log($"Current enemy count: {currentEnemyCount}");
            // Random interval before next spawn attempt
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator SpawnEnemiesOfType(EnemySpawnData enemyType, int spawnCount)
    {
        Debug.Log($"Spawning {spawnCount} of {enemyType.enemyId}");

        // Try to get enemy data if using the Enemy Manager system
        EnemyData enemyData = null;
        if (!string.IsNullOrEmpty(enemyType.enemyId) && EnemyManager.Instance != null)
        {
            Debug.Log($"Getting enemy data for {enemyType.enemyId}");
            enemyData = EnemyManager.Instance.GetEnemyData(enemyType.enemyId);
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPoint = FindRandomSpawnPoint();
            Debug.Log($"Spawn point found: {spawnPoint}");

            if (spawnPoint != Vector3.zero)
            {
                GameObject enemy = Instantiate(enemyType.enemyPrefab, spawnPoint, Quaternion.identity);
                spawnedEnemies.Add(enemy);
                currentEnemyCount++;

                // Apply enemy data if available
                if (enemyData != null)
                {
                    if (enemy.TryGetComponent<EntityStatus>(out var entityStatus))
                    {
                        entityStatus.maxHealth = enemyData.maxHealth;
                        entityStatus.Revive(); // Reset health to new max value
                    }

                    if (!enemy.TryGetComponent<EnemyIdentity>(out var identity))
                    {
                        identity = enemy.AddComponent<EnemyIdentity>();
                    }
                    identity.SetEnemyData(enemyData);

                    if (!enemy.TryGetComponent<EnemyXP>(out var enemyXP))
                    {
                        enemyXP = enemy.AddComponent<EnemyXP>();
                    }
                }

                    EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                    Debug.Log($"Enemy AI component found: {enemyAI != null}");
                    
                    if (enemyAI != null)
                    {
                        float patrolRadius = Random.Range(5f, 15f);
                        enemyAI.Initialize(player, spawnPoint, patrolRadius);
                        Debug.Log($"Enemy AI initialized with patrol radius: {patrolRadius}");
                    } 
                    else
                    {
                        Debug.LogError("EnemyAI component not found on the enemy prefab!");
                    }

                yield return new WaitForSeconds(0.1f); // Small delay between spawns
            }
        }
    }

    private Vector3 FindRandomSpawnPoint()
    {
        int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random point within the terrain bounds
            Vector3 randomPoint = new Vector3(
                Random.Range(0, targetTerrain.terrainData.size.x),
                0,
                Random.Range(0, targetTerrain.terrainData.size.z)
            );

            // Convert to world position
            randomPoint.x += targetTerrain.transform.position.x;
            randomPoint.z += targetTerrain.transform.position.z;
            randomPoint.y = targetTerrain.transform.position.y + 50f; // Start high above the terrain

            // Raycast down to find ground
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 100f))
            {
                Vector3 spawnPoint = hit.point + Vector3.up * 0.5f; // Slightly above ground

                // Check if the slope is too steep
                if (Vector3.Angle(hit.normal, Vector3.up) > maxSlopeAngle)
                {
                    continue; // Too steep, try again
                }

                // Check distance from player
                float distanceToPlayer = Vector3.Distance(spawnPoint, player.position);
                if (distanceToPlayer < minDistanceFromPlayer || distanceToPlayer > maxDistanceFromPlayer)
                {
                    continue; // Wrong distance, try again
                }

                // Check if this point is on a valid NavMesh
                if (NavMesh.SamplePosition(spawnPoint, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    return navHit.position;
                }
            }
        }

        return Vector3.zero; // No valid position found
    }

    private void CheckForDespawn()
    {
        if (player == null) return;

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = spawnedEnemies[i];

            if (enemy == null || !enemy.activeSelf)
            {
                spawnedEnemies.RemoveAt(i);
                continue;
            }

            // Check if enemy is too far from player
            if (Vector3.Distance(enemy.transform.position, player.position) > despawnDistance)
            {
                Destroy(enemy);
                spawnedEnemies.RemoveAt(i);
                currentEnemyCount--;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw terrain bounds
        if (targetTerrain != null)
        {
            Gizmos.color = Color.green;
            Vector3 terrainPos = targetTerrain.transform.position;
            Vector3 terrainSize = targetTerrain.terrainData.size;
            Gizmos.DrawWireCube(
                terrainPos + new Vector3(terrainSize.x / 2, terrainSize.y / 2, terrainSize.z / 2),
                terrainSize
            );
        }

        // Draw distances from player
        if (player != null)
        {
            // Despawn distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, despawnDistance);

            // Min spawn distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);

            // Max spawn distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        }
    }
}