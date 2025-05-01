using UnityEngine;
using UnityEngine.SceneManagement;
using World;

public static class BoostrapperHook
{
    private const string SceneName = "BootstrapScene";
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeMethodLoad()
    {
        for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            var scene = SceneManager.GetSceneAt(sceneIndex);
            if (scene.name != SceneName) continue;
            Debug.Log($"Bootstrap scene '{SceneName}' already loaded.");
            return;
        }
        SceneManager.LoadScene(SceneName);
    }
}

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private PixelPerfectScreenManager screenManager;
    [SerializeField] private WorldManager worldManager;
    [SerializeField] private GameAssetDatabase gameAssetDatabase;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private SceneGraphService sceneGraphService;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (FindFirstObjectByType<EntryPoint>() != this)
        {
            Debug.LogWarning("Multiple EntryPoint instances found. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {        
        if (!EnsureExists(ref gameManager))
        {
            Debug.LogError("GameManager not found in the scene.");
            return;
        }
        
        if (!EnsureExists(ref screenManager))
        {
            Debug.LogError("PixelPerfectScreenManager not found in the scene.");
            return;
        }
        
        if (!EnsureExists(ref worldManager))
        {
            Debug.LogError("WorldManager not found in the scene.");
            return;
        }
        
        if (!EnsureExists(ref playerManager))
        {
            Debug.LogError("PlayerManager not found in the scene.");
            return;
        }

        if (!EnsureExists(ref gameAssetDatabase))
        {
            Debug.LogError("GameAssetDatabase not found in the scene.");
            return;
        }
        
        if (!EnsureExists(ref sceneGraphService))
        {
            Debug.LogError("SceneGraphService not found in the scene.");
            return;
        }
        sceneGraphService.SetRootScene("Boot", "BootstrapScene");
        
        var context = new GameContext(
            screenManager,
            sceneGraphService,
            gameAssetDatabase,
            worldManager,
            playerManager
        );
        
        gameManager.Initialize(context);
    }
    
    private static bool EnsureExists<T>(ref T service) where T : Object
    {
        if (service)
        {
            return true;
        }
        service = FindFirstObjectByType<T>();
        return service;
    }
}
