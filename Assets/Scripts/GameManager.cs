using System;
using System.Threading.Tasks;
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
    

    public void StartNewGame(int slot = 0)
    {
        if (_currentGameState != GameState.MainMenu)
        {
            Log.Warning("Cannot start new game from current state: " + _currentGameState);
            return;
        }
            // overwrite the first save slot
        _currentSaveSlot = slot;

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
        var gameProgression = new GameProgression();
        return new GameSession(startSectionId, spawnPointId, playerStats, gameProgression);
    }

    private GameSession LoadGameSessionFromSave(SessionSaveData saveData)
    {
        if (saveData == null)
        {
            Log.Error("Save data is null.");
            return null;
        }

        var session = new GameSession();
        session.LoadData(saveData);

        return session;
    }
    
    public void PauseGame()
    {
        if (_currentGameState != GameState.InGame)
        {
            Log.Warning("Cannot pause game from current state: " + _currentGameState);
            return;
        }

        AudioListener.pause = true;
        Time.timeScale = 0;
        _currentGameState = GameState.Paused;
    }

    private async void StartWithGameSession(GameSession session)
    {
        _gameSession = session;
        try
        {
            await LoadingScreenManager.Instance.Show("Loading Game...");
            await _gameContext.SceneService.ReplaceScene("Main", "GameScene");

            PlayerManager.Initialize(_gameSession, AssetDb.playerPrefab);

            World.Initialize(_gameContext, _gameSession);

            World.OnLoaded += OnWorldLoaded;
            var section = AssetDb.GetWorldSection(_gameSession.worldSectionId);
            if (!section)
            {
                throw new Exception("World section not found: " + _gameSession.worldSectionId);
            }
            await World.StartSessionAtSection(section, _gameSession.spawnPointId);

            _currentGameState = GameState.InGame;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to start game session: " + ex.Message);

            await ReturnToMainMenu();
        }
    }
    
    public async void TransitionTo(string exitId)
    {
        try
        {
            if (_currentGameState != GameState.InGame)
            {
                Log.Warning("Cannot transition from current state: " + _currentGameState);
                return;
            }

            await LoadingScreenManager.Instance.Show("Transitioning...");
            
            World.TransitionTo(exitId);
        }
        catch (Exception ex)
        {
            Log.Error("Error during transition: " + ex.Message);
            await LoadingScreenManager.Instance.Hide(1f);
            await ReturnToMainMenu(); // fallback if transition fails
        }
    }
    
    private async void OnWorldLoaded(SectionLoadResult res)
    {
        try
        {
            var coordinator = res.SceneCoordinator;
            var spawnPoint = coordinator.GetSpawnPoint(_gameSession.spawnPointId);
            var spawnPosition = spawnPoint?.position ?? Vector3.zero;
            var spawnRotation = spawnPoint?.rotation ?? Quaternion.identity;
            PlayerManager.SpawnAt(spawnPosition, spawnRotation);
            coordinator.AssignCameraTarget(PlayerManager.Player.transform);
            await LoadingScreenManager.Instance.Hide(1f);
            SaveSystem.SaveGameSession(_gameSession, _currentSaveSlot);
            await LoadingScreenManager.Instance.ShowToastAsync("Progress saved", 2f);
        }
        catch (Exception e)
        {
            Log.Error("Error during world load: " + e.Message);
        }
    }
    
    public void ResumeGame()
    {
        if (_currentGameState != GameState.Paused)
        {
            Log.Warning("Cannot resume game from current state: " + _currentGameState);
            return;
        }

        AudioListener.pause = false;
        Time.timeScale = 1f;
        _currentGameState = GameState.InGame;
    }
    
    public async Task ReturnToMainMenu()
    {
        // if (_currentGameState == GameState.MainMenu) return;
        _currentGameState = GameState.MainMenu;
        await LoadingScreenManager.Instance.Show("Loading Main Menu...");
        World.OnLoaded -= OnWorldLoaded;
        
        SaveSystem.SaveGameSession(_gameSession, _currentSaveSlot);
        await World.Uninitialize();
        _gameSession = null;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        await _gameContext.SceneService.ReplaceScene("Main", "StartMenu");
        await LoadingScreenManager.Instance.Hide(1f);
    }
    
    public async Task QuitGame()
    {
        if (_currentGameState == GameState.InGame)
        {
            await ReturnToMainMenu();
        }
        Application.Quit();
    }
}