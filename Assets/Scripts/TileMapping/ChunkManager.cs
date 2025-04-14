using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
    public List<int> tiles = new();
}

public class Chunk
{
    public Vector2Int position;
    public bool isLoaded;
    public List<GameObject> tiles = new();
    internal GameObject chunkObject;
}

public class ChunkManager : MonoBehaviour
{
    public GameObject[] tilePrefabs; // Array of different tile types
    public string mapFile = ""; // Path to the JSON file
    public int chunkSize = 10;
    public int renderDistance = 2;
    public bool generateNavMesh = true;
    private NavMeshSurface navMeshSurface;
    private readonly float cleanupInterval = 30f; // Intervalle de nettoyage en secondes
    private float cleanupTimer = 0f; // Minuteur pour suivre le temps écoulé
    private Transform playerTransform;


    private readonly Dictionary<Vector2Int, Chunk> chunks = new();
    private MapData mapData;
    private Vector2Int lastPlayerChunk;

    internal void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (generateNavMesh)
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
                navMeshSurface.collectObjects = CollectObjects.All;
                navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            }
    }

    internal void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name} ({GetMapFilePath()})");
        LoadMap(GetMapFilePath());
        UpdateChunks();
    }

    private string GetMapFilePath()
    {
        return string.IsNullOrEmpty(mapFile)
            ? Path.Combine(Application.streamingAssetsPath, $"Maps/Generated{SceneManager.GetActiveScene().name}.json")
            : Path.Combine(Application.streamingAssetsPath, $"Maps/{mapFile}.json");
    }

    private void LoadMap(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        mapData = JsonUtility.FromJson<MapData>(json);

        if (mapData == null)
        {
            Debug.LogError("Failed to deserialize map data.");
        }
    }

    internal void Update()
    {
        Transform player = GetPlayerTransform();
        if (player == null) return;

        Vector2Int playerChunk = GetPlayerChunk(player.position);
        if (playerChunk != lastPlayerChunk)
        {
            lastPlayerChunk = playerChunk;
            UpdateChunks();
        }
    }

    internal void FixedUpdate()
    {
        cleanupTimer += Time.fixedDeltaTime;

        if (cleanupTimer >= cleanupInterval)
        {
            CleanupInactiveChunks();
            cleanupTimer = 0f;
        }
    }

    private Transform GetPlayerTransform()
    {
        if (playerTransform == null || playerTransform.gameObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player not found.");
            }
        }
        return playerTransform;
    }

    private Vector2Int GetPlayerChunk(Vector3 playerPosition)
    {
        return new Vector2Int((int)(playerPosition.x / chunkSize), (int)(playerPosition.z / chunkSize));
    }

    private void UpdateChunks()
    {
        List<Vector2Int> activeChunks = GetActiveChunks();
        LoadNewChunks(activeChunks);
        UnloadInactiveChunks(activeChunks);
    }

    private List<Vector2Int> GetActiveChunks()
    {
        List<Vector2Int> activeChunks = new();

        for (int dz = -renderDistance; dz <= renderDistance; dz++)
        {
            for (int dx = -renderDistance; dx <= renderDistance; dx++)
            {
                Vector2Int chunkPos = lastPlayerChunk + new Vector2Int(dx, dz);
                activeChunks.Add(chunkPos);
            }
        }
        return activeChunks;
    }

    private void LoadNewChunks(List<Vector2Int> activeChunks)
    {
        foreach (var chunkPos in activeChunks)
        {
            if (!chunks.ContainsKey(chunkPos))
            {
                LoadChunk(chunkPos);
            }
        }
    }

    private void UnloadInactiveChunks(List<Vector2Int> activeChunks)
    {
        List<Vector2Int> chunksToUnload = new();
        foreach (var chunk in chunks)
        {
            if (!activeChunks.Contains(chunk.Key))
            {
                chunksToUnload.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToUnload)
        {
            UnloadChunk(chunkPos);
        }
    }

        private void UpdateNavMesh()
    {
        // Reconstruire la NavMesh pour inclure les nouveaux chunks
        navMeshSurface.BuildNavMesh();
    }

    private void LoadChunk(Vector2Int chunkPos)
    {
        if (chunks.TryGetValue(chunkPos, out Chunk existingChunk) && !existingChunk.isLoaded)
        {
            // Réactiver un chunk désactivé
            existingChunk.chunkObject.SetActive(true);
            existingChunk.isLoaded = true;

            if (generateNavMesh)
            {
                UpdateNavMesh();
            }
            return;
        }

        // Si aucun chunk désactivé n'existe, en créer un nouveau
        Chunk newChunk = CreateNewChunk(chunkPos);
        PopulateChunkWithTiles(newChunk);
        chunks[chunkPos] = newChunk;

        if (generateNavMesh)
            UpdateNavMesh();
    }

    private Chunk CreateNewChunk(Vector2Int chunkPos)
    {
        Chunk newChunk = new()
        {
            position = chunkPos,
            isLoaded = true,
            chunkObject = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}")
        };
        newChunk.chunkObject.transform.parent = transform;
        return newChunk;
    }

    private void PopulateChunkWithTiles(Chunk newChunk)
    {
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                // Calcul des coordonnées mondiales
                int worldX = newChunk.position.x * chunkSize + x;
                int worldZ = newChunk.position.y * chunkSize + z;

                // Vérification des limites pour éviter les erreurs
                if (worldX < 0 || worldZ < 0 || worldX >= mapData.width || worldZ >= mapData.height) continue;

                // Calcul de l'index dans la liste des tuiles
                int index = worldZ * mapData.width + worldX;

                // Vérification de l'index pour éviter les dépassements
                if (index < 0 || index >= mapData.tiles.Count) continue;

                // Décodage des informations de la tuile
                int packedValue = mapData.tiles[index];
                int tileType = packedValue & 0b111; // Les 3 derniers bits pour le type de tuile
                int rotationIndex = (packedValue >> 3) & 0b11; // Les 2 bits suivants pour la rotation
                int height = packedValue >> 5; // Les bits restants pour la hauteur

                // Vérification du type de tuile
                if (tileType < 0 || tileType >= tilePrefabs.Length) continue;

                // Création et positionnement de la tuile
                Vector3 position = new(worldX, height, worldZ);
                Quaternion rotation = Quaternion.Euler(0, rotationIndex * 90, 0); // Appliquer la rotation
                GameObject tile = Instantiate(tilePrefabs[tileType], position, rotation, newChunk.chunkObject.transform);
                newChunk.tiles.Add(tile);
                if (tile.GetComponent<Collider>() == null)
                {
                    tile.AddComponent<BoxCollider>();
                }

                // Debug.Log($"Tuile instanciée à {position} avec type {tileType}, hauteur {height}, rotation {rotationIndex * 90}°");
            }
        }
    }

    private void UnloadChunk(Vector2Int chunkPos)
    {
        if (chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            chunk.chunkObject.SetActive(false);
            chunk.isLoaded = false;
        }
    }

    private void CleanupInactiveChunks()
    {
        List<Vector2Int> chunksToRemove = new();
        foreach (var chunk in chunks)
        {
            if (!chunk.Value.isLoaded)
            {
                Destroy(chunk.Value.chunkObject);
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            chunks.Remove(chunkPos);
        }
    }
}
