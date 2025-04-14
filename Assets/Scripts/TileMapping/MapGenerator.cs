using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] tilePrefabs;  // Array of tile prefabs for different types
    public int width = 5;             // Width of the map
    public int height = 5;            // Height of the map
    public float scale = 0.1f;        // Scale of Perlin Noise (affects frequency)
    public int heightRange = 8;       // Max height value
    public string sceneName = "";    // Name of the map
    private MapData mapData;

    internal void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    internal void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, $"Maps/Generated{sceneName}.json");
        if (File.Exists(filePath))
            return;

        GenerateMap();

        if (mapData == null)
        {
            Debug.LogError("mapData is NULL !");
            return;
        }

        ExportMapToJSON(filePath);
    }

    private void GenerateMap()
    {
        InitializeMapData();

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int packedValue = GenerateTileValue(x, z);
                mapData.tiles[z * width + x] = packedValue;
            }
        }
    }

    private void InitializeMapData()
    {
        mapData = new MapData
        {
            width = width,
            height = height,
            tiles = new List<int>(new int[width * height])
        };
    }

    private int GenerateTileValue(int x, int z)
    {
        float xCoord = x * scale;
        float zCoord = z * scale;
        float perlinValue = Mathf.PerlinNoise(xCoord, zCoord); // Generate height (0 to 1)

        int heightValue = Mathf.FloorToInt(perlinValue * heightRange);
        int tileType = Random.Range(0, tilePrefabs.Length);  // Random tile type
        return (heightValue << 3) | (tileType & 0b111); // Pack height and tile type
    }

    private void ExportMapToJSON(string filePath)
    {
        string json = JsonUtility.ToJson(mapData, true);

        File.WriteAllText(filePath, json);
    }
}
