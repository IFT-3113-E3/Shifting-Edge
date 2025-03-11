using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private readonly Dictionary<Vector2Int, Chunk> chunks = new();
    private MapData mapData;
    private Vector2Int lastPlayerChunk;

    internal void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
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

    private Transform GetPlayerTransform()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found.");
        }
        return player;
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

    private void LoadChunk(Vector2Int chunkPos)
    {
        if (chunks.ContainsKey(chunkPos)) return;

        Chunk newChunk = CreateNewChunk(chunkPos);
        PopulateChunkWithTiles(newChunk);
        chunks[chunkPos] = newChunk;
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
                int worldX = newChunk.position.x * chunkSize + x;
                int worldZ = newChunk.position.y * chunkSize + z;

                if (worldX >= mapData.width || worldZ >= mapData.height) continue;

                int index = worldZ * mapData.width + worldX;
                if (index < 0 || index >= mapData.tiles.Count) continue;

                int packedValue = mapData.tiles[index];
                int tileType = packedValue & 0b111;
                int height = packedValue >> 3;

                if (tileType < 0 || tileType >= tilePrefabs.Length) continue;

                Vector3 position = new(worldX, height, worldZ);
                GameObject tile = Instantiate(tilePrefabs[tileType], position, Quaternion.identity, newChunk.chunkObject.transform);
                newChunk.tiles.Add(tile);
            }
        }
    }

    private void UnloadChunk(Vector2Int chunkPos)
    {
        if (chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            foreach (var tile in chunk.tiles)
            {
                Destroy(tile);
            }
            Destroy(chunk.chunkObject);
            chunks.Remove(chunkPos);
        }
    }
}
