using System;
using UnityEngine;
using Unity.Logging;
using UnityEngine.SceneManagement;
using World;

public enum GameState
{
    MainMenu,
    InGame,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Initialized fields
    private GameContext _gameContext;

    // Uninitialized fields
    private GameSession _gameSession;
    
    private GameState _currentGameState;

    private int _currentSaveSlot;
    public GameSession GameSession => _gameSession;
    public WorldManager World => _gameContext.WorldManager;
    public PlayerManager PlayerManager => _gameContext.PlayerManager;
    public GameAssetDatabase AssetDb => _gameContext.AssetDatabase;
    public bool IsInGame => _currentGameState == GameState.InGame && _gameSession != null;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Log.Warning("Multiple GameManager in scene!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Log.Info("Game Manager has been instantiated.");
    }

    private void Start()
    {
        Log.Info("Starting Game Manager");
    }
    
    public void Initialize(GameContext gameContext)
    {
        if (Instance != this)
        {
            Log.Error("GameManager instance mismatch.");
            return;
        }
        
        if (gameContext == null)
        {
            Log.Error("GameContext is null.");
            return;
        }

        _gameContext = gameContext;
        
        _ = _gameContext.SceneService.PushChildScene("Boot", "Main", "StartMenu");
        _currentGameState = GameState.MainMenu;
    }
    
    private void Update()
    {
        if (_currentGameState == GameState.InGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }
        else if (_currentGameState == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
        }
    }
    
    public void OnPlayerDeath()
    {
        if (_currentGameState != GameState.InGame) return;
        
        _currentGameState = GameState.GameOver;
        PlayerManager.Despawn();
    }
    
    public void LoadGame(int slot)
    {
        if (_currentGameState != GameState.MainMenu)
        {
            Log.Warning("Cannot start game from current state: " + _currentGameState);
            return;
        }
        if (!SaveSystem.SaveExists(slot)) 
        {
            Log.Warning("No save data found in slot " + slot);
            return;
        }
        
        // Load the game session
        var saveData = SaveSystem.LoadSaveData(slot);
        var gameSession = LoadGameSessionFromSave(saveData);
        
        if (gameSession == null)
        {
            Log.Error("Failed to load game session.");
            return;
        }

        StartWithGameSession(gameSession);
    }
    

    public void StartNewGame()
    {
        if (_currentGameState != GameState.MainMenu)
        {
            Log.Warning("Cannot start new game from current state: " + _currentGameState);
            return;
        }
        if (!SaveSystem.GetNextSaveSlot(out _currentSaveSlot)) 
        {
            Log.Error("Failed to get next save slot.");
            return;
        }

        _gameSession = CreateGameSession();
        
        if (_gameSession == null)
        {
            Log.Error("Failed to create new game session.");
            return;
        }
        
        StartWithGameSession(_gameSession);
    }

    private GameSession CreateGameSession()
    {
        string startSectionId = AssetDb.GetWorldSection("ice1").sectionId;
        string spawnPointId = "SpawnPoint";
        PlayerStats playerStats = ScriptableObject.CreateInstance<PlayerStats>();
        return new GameSession(startSectionId, spawnPointId, playerStats);
    }

    private GameSession LoadGameSessionFromSave(SessionSaveData saveData)
    {
        if (saveData == null)
        {
            Log.Error("Save data is null.");
            return null;
        }

        var worldSectionId = saveData.worldSectionId;
        var spawnPointId = saveData.spawnPointId;

        var playerStats = ScriptableObject.CreateInstance<PlayerStats>();
        playerStats.currentHealth = saveData.currentHealth;
        playerStats.maxHealth = saveData.maxHealth;

        return new GameSession(worldSectionId, spawnPointId, playerStats);
    }
    
    public void PauseGame()
    {
        if (_currentGameState != GameState.InGame)
        {
            Log.Warning("Cannot pause game from current state: " + _currentGameState);
            return;
        }

        Time.timeScale = 0;
        _currentGameState = GameState.Paused;
    }

    private async void StartWithGameSession(GameSession session)
    {
        _gameSession = session;
        bool sceneReplaced = false;
        bool playerInitialized = false;
        bool worldInitialized = false;

        try
        {
            await _gameContext.SceneService.ReplaceScene("Main", "GameScene");
            sceneReplaced = true;

            PlayerManager.Initialize(_gameSession, AssetDb.playerPrefab);
            playerInitialized = true;

            World.Initialize(_gameContext, _gameSession);
            worldInitialized = true;

            World.OnLoaded += OnWorldLoaded;
            World.StartSessionAtSection(AssetDb.GetWorldSection(_gameSession.worldSectionId), _gameSession.spawnPointId);

            _currentGameState = GameState.InGame;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to start game session: " + ex.Message);

            // Cleanup in reverse order
            if (worldInitialized)
                World.Uninitialize();

            if (playerInitialized)
                PlayerManager.Despawn();

            if (sceneReplaced)
                await _gameContext.SceneService.ReplaceScene("Main", "StartMenu");

            _gameSession = null;
            _currentGameState = GameState.MainMenu;
        }
    }
    
    private void OnWorldLoaded(SectionLoadResult res)
    {
        try
        {
            var coordinator = res.SceneCoordinator;
            var spawnPoint = coordinator.GetSpawnPoint(_gameSession.spawnPointId);
            var spawnPosition = spawnPoint?.position ?? Vector3.zero;
            var spawnRotation = spawnPoint?.rotation ?? Quaternion.identity;
            PlayerManager.SpawnAt(spawnPosition, spawnRotation);
            coordinator.AssignCameraTarget(PlayerManager.Player.transform);
        }
        catch (Exception ex)
        {
            Log.Error("Error while spawning player: " + ex.Message);
            ReturnToMainMenu(); // fallback if spawn fails
        }
    }
    
    public void ResumeGame()
    {
        if (_currentGameState != GameState.Paused)
        {
            Log.Warning("Cannot resume game from current state: " + _currentGameState);
            return;
        }

        Time.timeScale = 1;
        _currentGameState = GameState.InGame;
    }
    
    public void ReturnToMainMenu()
    {
        World.OnLoaded -= OnWorldLoaded;
        SaveSystem.SaveGameSession(_gameSession, _currentSaveSlot);
        World.Uninitialize();
        _gameSession = null;
        
        // SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
        _ = _gameContext.SceneService.ReplaceScene("Main", "StartMenu");
    }
    
    public void QuitGame()
    {
        if (_currentGameState == GameState.InGame)
        {
            ReturnToMainMenu();
        }
        Application.Quit();
    }
}