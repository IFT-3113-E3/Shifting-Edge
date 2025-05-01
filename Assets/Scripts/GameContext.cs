using Unity.Logging;
using World;

public class GameContext
{
    public PixelPerfectScreenManager ScreenManager { get; }
    public SceneGraphService SceneService { get; }
    public GameAssetDatabase AssetDatabase { get; }
    public WorldManager WorldManager { get; }
    public PlayerManager PlayerManager { get; }
        
    public GameContext(
        PixelPerfectScreenManager screenManager,
        SceneGraphService sceneService,
        GameAssetDatabase assetDatabase,
        WorldManager worldManager,
        PlayerManager playerManager)
    {
        ScreenManager = screenManager;
        SceneService = sceneService;
        AssetDatabase = assetDatabase;
        WorldManager = worldManager;
        PlayerManager = playerManager;
    }
}