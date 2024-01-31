using JetBrains.Profiler.Api;
using JetBrains.Profiler.SelfApi;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Debugging.Profiling;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Modding;
using Korpi.Client.Registries;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.UI;
using Korpi.Client.UI.HUD;
using Korpi.Client.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Korpi.Client.Window;

/// <summary>
/// The main game client window.
/// </summary>
public class GameClient : GameWindow
{
    private static readonly Logging.IKorpiLogger Logger = Logging.LogFactory.GetLogger(typeof(GameClient));
    
    /// <summary>
    /// Called when the game client is resized.
    /// </summary>
    public static event Action? ClientResized;

    public static int WindowWidth { get; private set; }
    public static int WindowHeight { get; private set; }
    public static float WindowAspectRatio { get; private set; }
    public static bool IsPlayerInGui { get; private set; }

    private ImGuiController _imGuiController = null!;
    private ShaderManager _shaderManager = null!;
    private GameWorld _gameWorld = null!;
    private GameWorldRenderer _gameWorldRenderer = null!;
    private Crosshair _crosshair = null!;
    private PlayerEntity _playerEntity = null!;

    private double _fixedFrameAccumulator;

#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif


    public GameClient() : base(
        new GameWindowSettings
        {
            UpdateFrequency = Constants.UPDATE_FRAME_FREQUENCY
        },
        new NativeWindowSettings
        {
            Size = new Vector2i(ClientConfig.WindowConfig.WindowWidth, ClientConfig.WindowConfig.WindowHeight),
            Title = $"{Constants.CLIENT_NAME} v{Constants.CLIENT_VERSION}",
            NumberOfSamples = 0,
            Location = new Vector2i(200, 0),
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4,2),
            // WindowState = WindowState.Fullscreen,
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        }) { }


    protected override void OnLoad()
    {
        base.OnLoad();
        Logger.Info($"Starting v{Constants.CLIENT_VERSION}...");

        if (ClientConfig.Store.EnableSelfProfile)
        {
            Logger.Warn("Initializing DotTrace... (this may take a while)");
            DotTrace.EnsurePrerequisite();   // Initialize the DotTrace API and download the profiler tool (if needed).
            DotTrace.Config cfg = new DotTrace.Config().SaveToFile(ClientConfig.Store.SelfProfileOutputFilePath);
            DotTrace.Attach(cfg);   // Attach the profiler to the current process.
            DotTrace.StartCollectingData();  // Start collecting data.
            Logger.Warn($"DotTrace initialized. Profile output will be saved to {ClientConfig.Store.SelfProfileOutputFilePath}.");
        }
        Logger.Info($"MainThread ID={Environment.CurrentManagedThreadId}");
        
        WindowWidth = ClientSize.X;
        WindowHeight = ClientSize.Y;
        WindowAspectRatio = ClientSize.X / (float)ClientSize.Y;

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
        _shaderManager = new ShaderManager();

        // World initialization.
        GameTime.Initialize();
        _gameWorld = new GameWorld("World1");
        _gameWorldRenderer = new GameWorldRenderer(_gameWorld);

        // PlayerEntity initialization.
        _playerEntity = new PlayerEntity(new Vector3(0, Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 2f, 0), 0, 0);
#if DEBUG
        if (ClientConfig.DebugModeConfig.IsPhotoModeEnabled)
            PhotoModeCamera.Create(new Vector3(0, 256, 48), -30, -100);
#endif
        CursorState = CursorState.Grabbed;

        // UI initialization.
        _crosshair = new Crosshair();
        _imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        ImGuiWindowManager.CreateDefaultWindows();

        Logger.Info("Started.");
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        Logger.Info("Shutting down...");
        _crosshair.Dispose();
        _shaderManager.Dispose();
        _gameWorldRenderer.Dispose();
        _imGuiController.DestroyDeviceObjects();
        TextureRegistry.BlockArrayTexture.Dispose();
        ImGuiWindowManager.Dispose();
        GlobalJobPool.Shutdown();
        _playerEntity.Disable();

        if (ClientConfig.Store.EnableSelfProfile)
        {
            DotTrace.SaveData();
            DotTrace.Detach();   // Detach the profiler from the current process.
            Logger.Warn($"DotTrace profile output saved to {ClientConfig.Store.SelfProfileOutputFilePath}.");
        }
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
        GL.PolygonMode(MaterialFace.FrontAndBack, ClientConfig.DebugModeConfig.RenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
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
        
        using (new ProfileScope("DrawWorld"))
            DrawWorld();

        using (new ProfileScope("DrawUi"))
            DrawUi();
        // float averageFps = ImGuiNET.ImGui.GetIO().Framerate;
        // Console.WriteLine($"fps: {averageFps}");
        // if (GameTime.TotalTime > 2 && !profile && averageFps < 200)
        // {
        //     MeasureProfiler.StartCollectingData();
        //     profile = true;
        //     Console.WriteLine("Started profiling");
        // }
        // if (profile && averageFps > 500)
        // {
        //     MeasureProfiler.SaveData();
        //     MeasureProfiler.Detach();
        //     profile = false;
        //     Console.WriteLine("Stopped profiling");
        // }

        if (Input.KeyboardState.IsKeyPressed(Keys.F2))
            ScreenshotUtility.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng("Screenshots");

        SwapBuffers();
        KorpiProfiler.End();
        KorpiProfiler.EndFrame();
    }
    // private bool profile = false;


    private void DrawUi()
    {
#if DEBUG
        if (ClientConfig.DebugModeConfig.RenderCrosshair)
#endif
            _crosshair.Draw();

        DrawImGui();
    }


    private void DrawWorld()
    {
        _gameWorldRenderer.Draw();

#if DEBUG
        if (ClientConfig.DebugModeConfig.IsPhotoModeEnabled && GameTime.TotalTime > 1f && DebugStats.ChunksInGenerationQueue == 0 && DebugStats.ChunksInMeshingQueue == 0)
        {
            ScreenshotUtility.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng(ClientConfig.DebugModeConfig.PhotoModeScreenshotPath, "latest", true, true);
            Logger.Debug($"Photomode completed in {GameTime.TotalTime:F2}s.");
            Close();
            return;
        }
#endif

        DebugDrawer.Draw();
    }


    /// <summary>
    /// Fixed update loop.
    /// Called <see cref="Constants.FIXED_UPDATE_FRAME_FREQUENCY"/> times per second.
    /// </summary>
    private void FixedUpdate()
    {
        GameTime.FixedUpdate();
        GlobalJobPool.FixedUpdate();
        _gameWorld.FixedUpdate();
    }


    /// <summary>
    /// Regular update loop.
    /// Called every frame.
    /// </summary>
    private void Update()
    {
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

        // MousePosition = new Vector2(ClientSize.X / 2f, ClientSize.Y / 2f);

        _imGuiController.Update(this, GameTime.DeltaTimeFloat, mousePos);

        ImGuiWindowManager.UpdateAllWindows();

        if (Input.KeyboardState.IsKeyPressed(Keys.Escape))
            SwitchCursorState();
    }


    private void DrawImGui()
    {
        _imGuiController.Render();
        ImGuiController.CheckGlError("End of frame");
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        WindowWidth = e.Width;
        WindowHeight = e.Height;
        WindowAspectRatio = e.Width / (float)e.Height;

        GL.Viewport(0, 0, WindowWidth, WindowHeight);

        // Tell ImGui of the new window size.
        _imGuiController.WindowResized(ClientSize.X, ClientSize.Y);
        ClientResized?.Invoke();
    }


    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        _imGuiController.PressChar((char)e.Unicode);
    }


    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        _imGuiController.MouseScroll(e.Offset);
    }


    private void SwitchCursorState()
    {
#if DEBUG
        if (ClientConfig.DebugModeConfig.IsPhotoModeEnabled)
            return;
#endif

        if (CursorState == CursorState.Grabbed)
        {
            CursorState = CursorState.Normal;
            IsPlayerInGui = true;
        }
        else if (CursorState == CursorState.Normal)
        {
            CursorState = CursorState.Grabbed;
            IsPlayerInGui = false;
        }
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