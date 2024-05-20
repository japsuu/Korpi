using JetBrains.Profiler.SelfApi;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Meshing;
using Korpi.Client.Modding;
using Korpi.Client.Player;
using Korpi.Client.Registries;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.UI.HUD;
using Korpi.Client.World;
using KorpiEngine.Core;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Windowing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Cursor = KorpiEngine.Core.InputManagement.Cursor;

namespace Korpi.Client;

public class KorpiGame : Game
{
    private GameClient _client = null!;
    private GameWorld _gameWorld = null!;
    private GameWorldRenderer _gameWorldRenderer = null!;
    private Crosshair _crosshair = null!;
    private PlayerEntity _playerEntity = null!;


    public KorpiGame(WindowingSettings settings) : base(settings) { }


    protected override void LoadContent()
    {
        Logger.Info($"Starting v{Constants.CLIENT_VERSION}...");

        // Create the game client.
        _client = new GameClient();

        if (ClientConfig.Profiling.EnableSelfProfile)
        {
            Logger.Warn("Initializing DotTrace... (this may take a while)");
            DotTrace.EnsurePrerequisite();   // Initialize the DotTrace API and download the profiler tool (if needed).
            DotTrace.Config cfg = new DotTrace.Config().SaveToFile(ClientConfig.Profiling.SelfProfileTargetPath);
            DotTrace.Attach(cfg);   // Attach the profiler to the current process.
            DotTrace.StartCollectingData();  // Start collecting data.
            Logger.Warn($"DotTrace initialized. Profile output will be saved to {ClientConfig.Profiling.SelfProfileTargetPath}.");
        }
        
        Cursor.SetGrabbed(true);

        // Resource initialization.
        BlockTextureRegistry.StartTextureRegistration();
        ModLoader.LoadAllMods();
        BlockTextureRegistry.FinishTextureRegistration();
        ShaderManager.Initialize();

        // World initialization.
        GameTime.Initialize();
        _gameWorld = new GameWorld("World1");
        _gameWorldRenderer = new GameWorldRenderer(_gameWorld);

        // PlayerEntity initialization.
        _playerEntity = new PlayerEntity(new Vector3(0, Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 2f, 0), 0, 0);

        // UI initialization.
        _crosshair = new Crosshair();
        
        // Restore window fullscreen state from config.
        Window.WindowState = ClientConfig.Window.Fullscreen ? WindowState.Fullscreen : WindowState.Normal;

        Logger.Info("Started.");
    }


    protected override void UnloadContent()
    {
        Logger.Info("Shutting down...");
        SaveConfigs();
        _crosshair.Dispose();
        _gameWorldRenderer.Dispose();
        _playerEntity.Disable();
        ChunkMesher.Dispose();
        BlockTextureRegistry.BlockArrayTexture.Dispose();
        ShaderManager.Dispose();

        if (ClientConfig.Profiling.EnableSelfProfile)
        {
            DotTrace.SaveData();
            DotTrace.Detach();   // Detach the profiler from the current process.
            Logger.Warn($"DotTrace profile output saved to {ClientConfig.Profiling.SelfProfileTargetPath}.");
        }
    }


    protected override void Render()
    {
#if DEBUG
        // Set the polygon mode to wireframe if the debug setting is enabled.
        GL.PolygonMode(MaterialFace.FrontAndBack, ClientConfig.Rendering.Debug.RenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
#endif
        
        _gameWorldRenderer.Draw();

        DebugDrawer.Draw();

        _crosshair.Draw();

        if (Input.KeyboardState.IsKeyPressed(Keys.F2))
            ScreenshotUtility.CaptureFrame(Window.ClientSize.X, Window.ClientSize.Y).SaveAsPng("Screenshots");
    }


    protected override void FixedUpdate()
    {
        _gameWorld.FixedUpdate();
        DebugStats.CalculateStats();
    }


    protected override void Update()
    {
        // Check for toggle cursor grab state.
        if (Input.KeyboardState.IsKeyPressed(Keys.Escape))
            Cursor.ChangeGrabState();
        
        // Check for fullscreen toggle.
        if (Input.KeyboardState.IsKeyPressed(Keys.F11))
            Window.WindowState = Window.IsFullscreen ? WindowState.Normal : WindowState.Fullscreen;
        
        GameTime.Update();
        _gameWorld.Update();
        
        if (Input.KeyboardState.IsKeyPressed(Keys.F10))
            ShaderManager.ReloadAllShaderPrograms();
    }


    private void SaveConfigs()
    {
        if (!Window.IsFullscreen)
        {
            ClientConfig.Window.WindowWidth = Window.ClientSize.X;
            ClientConfig.Window.WindowHeight = Window.ClientSize.Y;
        }
        ClientConfig.Window.Fullscreen = Window.IsFullscreen;
    }
}