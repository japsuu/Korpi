using JetBrains.Profiler.SelfApi;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Debugging.Profiling;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Meshing;
using Korpi.Client.Modding;
using Korpi.Client.Registries;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.SceneManagement;
using Korpi.Client.SceneManagement.Scenes;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.UI;
using Korpi.Client.UI.HUD;
using Korpi.Client.World;
using KorpiEngine.Core.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Korpi.Client;

/// <summary>
/// The main client window.
/// </summary>
public class ClientWindow : GameWindow
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ClientWindow));
    
    public static event Action? Disposing;

    private GameClient _client = null!;
    private ImGuiController _imGuiController = null!;
    private GameWorld _gameWorld = null!;
    private GameWorldRenderer _gameWorldRenderer = null!;
    private Crosshair _crosshair = null!;
    private PlayerEntity _playerEntity = null!;
    private double _fixedFrameAccumulator;

#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif


    public ClientWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }


    protected override void OnLoad()
    {
        base.OnLoad();
        Logger.Info($"Starting v{Constants.CLIENT_VERSION}...");

        // Create the game client.
        _client = new GameClient();
        
        // Load the main menu scene.
        SceneManager.LoadScene(new MainMenuScene(_client));

        if (ClientConfig.Profiling.EnableSelfProfile)
        {
            Logger.Warn("Initializing DotTrace... (this may take a while)");
            DotTrace.EnsurePrerequisite();   // Initialize the DotTrace API and download the profiler tool (if needed).
            DotTrace.Config cfg = new DotTrace.Config().SaveToFile(ClientConfig.Profiling.SelfProfileTargetPath);
            DotTrace.Attach(cfg);   // Attach the profiler to the current process.
            DotTrace.StartCollectingData();  // Start collecting data.
            Logger.Warn($"DotTrace initialized. Profile output will be saved to {ClientConfig.Profiling.SelfProfileTargetPath}.");
        }
        
        SystemInfo.Initialize();
        WindowInfo.Initialize(this);
        Client.Cursor.Initialize(this);
        Client.Cursor.SetGrabbed(true);

#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        GL.Enable(EnableCap.DepthTest);     // Enable depth testing.
        // GL.Enable(EnableCap.Multisample);   // Enable multisampling.
        GL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);

        // Resource initialization.
        GlobalJobPool.Initialize();
        TextureRegistry.StartTextureRegistration();
        ModLoader.LoadAllMods();
        TextureRegistry.FinishTextureRegistration();
        ShaderManager.Initialize();

        // World initialization.
        GameTime.Initialize();
        _gameWorld = new GameWorld("World1");
        _gameWorldRenderer = new GameWorldRenderer(_gameWorld);

        // PlayerEntity initialization.
        _playerEntity = new PlayerEntity(new Vector3(0, Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 2f, 0), 0, 0);

        // UI initialization.
        _crosshair = new Crosshair();
        _imGuiController = new ImGuiController(this);
        ImGuiWindowManager.CreateDefaultWindows();
        
        // Show the window after all resources are loaded.
        CenterWindow();
        IsVisible = true;
        
        // Restore window fullscreen state from config.
        WindowState = ClientConfig.Window.Fullscreen ? WindowState.Fullscreen : WindowState.Normal;

        Logger.Info("Started.");
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        Logger.Info("Shutting down...");
        SaveConfigs();
        _crosshair.Dispose();
        _gameWorldRenderer.Dispose();
        _playerEntity.Disable();
        ChunkMesher.Dispose();
        TextureRegistry.BlockArrayTexture.Dispose();
        ImGuiWindowManager.Dispose();
        GlobalJobPool.Shutdown();

        if (ClientConfig.Profiling.EnableSelfProfile)
        {
            DotTrace.SaveData();
            DotTrace.Detach();   // Detach the profiler from the current process.
            Logger.Warn($"DotTrace profile output saved to {ClientConfig.Profiling.SelfProfileTargetPath}.");
        }
        
        Disposing?.Invoke();
    }


    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        KorpiProfiler.BeginFrame();
        KorpiProfiler.Begin("UpdateLoop");
        base.OnUpdateFrame(args);
        
        double deltaTime = args.Time;
        _fixedFrameAccumulator += deltaTime;
        
        // DynamicPerformance.Update(deltaTime);
 
        using (new ProfileScope("FixedUpdate"))
        {
            while (_fixedFrameAccumulator >= Constants.FIXED_DELTA_TIME)
            {
                GameTime.FixedUpdate();
                FixedUpdate();
                _fixedFrameAccumulator -= Constants.FIXED_DELTA_TIME;
            }
        }
 
        double fixedAlpha = _fixedFrameAccumulator / Constants.FIXED_DELTA_TIME;
        
        if (deltaTime > Constants.MAX_DELTA_TIME)
        {
            Logger.Warn($"Detected large frame hitch ({1f/deltaTime:F2}fps, {deltaTime:F2}s)! Delta time was clamped to {Constants.MAX_DELTA_TIME:F2} seconds.");
            deltaTime = Constants.MAX_DELTA_TIME;
        }
        else if (deltaTime > Constants.DELTA_TIME_SLOW_THRESHOLD)
        {
            Logger.Warn($"Detected frame hitch ({deltaTime:F2}s)!");
            deltaTime = Constants.MAX_DELTA_TIME;
        }
        
        GameTime.Update(deltaTime, fixedAlpha);
        Input.Update(KeyboardState, MouseState);

        using (new ProfileScope("Update"))
            Update();
        KorpiProfiler.End();
    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        KorpiProfiler.Begin("DrawLoop");
        base.OnRenderFrame(args);
        
#if DEBUG
        // Set the polygon mode to wireframe if the debug setting is enabled.
        GL.PolygonMode(MaterialFace.FrontAndBack, ClientConfig.Rendering.Debug.RenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
#endif

        // Pass all of these matrices to the vertex shaders.
        // We could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.
        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        ShaderManager.UpdateViewMatrix(Camera.RenderingCamera.ViewMatrix);
        ShaderManager.UpdateProjectionMatrix(Camera.RenderingCamera.ProjectionMatrix);
        
        using (new ProfileScope("DrawScene"))
            SceneManager.Draw();
        
        using (new ProfileScope("DrawWorld"))
            DrawWorld();

        using (new ProfileScope("DrawUi"))
            DrawUi();

        if (Input.KeyboardState.IsKeyPressed(Keys.F2))
            ScreenshotUtility.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng("Screenshots");

        SwapBuffers();
        KorpiProfiler.End();
        KorpiProfiler.EndFrame();
    }


    private void DrawUi()
    {
        _crosshair.Draw();

        DrawImGui();
    }


    private void DrawWorld()
    {
        _gameWorldRenderer.Draw();

        DebugDrawer.Draw();
    }


    /// <summary>
    /// Fixed update loop.
    /// Called <see cref="Constants.FIXED_UPDATE_FRAME_FREQUENCY"/> times per second.
    /// </summary>
    private void FixedUpdate()
    {
        SceneManager.FixedUpdate();
        GlobalJobPool.FixedUpdate();
        _gameWorld.FixedUpdate();
        DebugStats.CalculateStats();
    }


    /// <summary>
    /// Regular update loop.
    /// Called every frame.
    /// </summary>
    private void Update()
    {
        SceneManager.Update();
        UpdateGui();
        
        using (new ProfileScope("GlobalJobPool.Update"))
            GlobalJobPool.Update();
        
        using (new ProfileScope("GameWorld.Update"))
            _gameWorld.Update();
        
        if (Input.KeyboardState.IsKeyPressed(Keys.F10))
            ShaderManager.ReloadAllShaderPrograms();
    }


    private void UpdateGui()
    {
        // Emulate the cursor being fixed to center of the screen, as OpenTK doesn't fix the cursor position when it's grabbed.
        Vector2 mousePos = Input.MouseState.Position;
        if (CursorState == CursorState.Grabbed)
            mousePos = new Vector2(ClientSize.X / 2f, ClientSize.Y / 2f);

        _imGuiController.Update(GameTime.DeltaTimeFloat, mousePos);

        ImGuiWindowManager.DrawAllWindows();

        if (Input.KeyboardState.IsKeyPressed(Keys.Escape))
            Client.Cursor.ChangeGrabState();
    }


    private void DrawImGui()
    {
        _imGuiController.Render();
        ImGuiController.CheckGlError("End of frame");
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }


    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        
        // Check for fullscreen toggle.
        if (e.Key == Keys.F11)
            WindowState = IsFullscreen ? WindowState.Normal : WindowState.Fullscreen;
    }


    private void SaveConfigs()
    {
        if (!IsFullscreen)
        {
            ClientConfig.Window.WindowWidth = ClientSize.X;
            ClientConfig.Window.WindowHeight = ClientSize.Y;
        }
        ClientConfig.Window.Fullscreen = IsFullscreen;
    }


#if DEBUG
    private static void OnDebugMessage(
        DebugSource source, // Source of the debugging message.
        DebugType type, // Type of the debugging message.
        int id, // ID associated with the message.
        DebugSeverity severity, // Severity of the message.
        int length, // Length of the string in pMessage.
        IntPtr pMessage, // Pointer to message string.
        IntPtr pUserParam)
    {
        if (severity == DebugSeverity.DebugSeverityNotification)
            return;
        
        // In order to access the string pointed to by pMessage, you can use Marshal
        // class to copy its contents to a C# string without unsafe code. You can
        // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
        string message = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(pMessage, length);
        
        Logger.OpenGl($"[{severity} source={source} type={type} id={id}] {message}");

        if (type == DebugType.DebugTypeError)
            throw new Exception(message);
    }
#endif
}