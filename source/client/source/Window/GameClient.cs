using System.Runtime.InteropServices;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Logging;
using Korpi.Client.Modding;
using Korpi.Client.Registries;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Rendering.Skybox;
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
    /// <summary>
    /// Called before <see cref="OnLoad"/> is exited.
    /// </summary>
    public static event Action? ClientLoad;

    /// <summary>
    /// Called before <see cref="OnUnload"/> is exited.
    /// </summary>
    public static event Action? ClientUnload;

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
    private Skybox _skybox = null!;
    private GameWorld _gameWorld = null!;
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
            NumberOfSamples = 8,
            Location = new Vector2i(0, 0),
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        }) { }


    protected override void OnLoad()
    {
        base.OnLoad();
        Logger.Log($"Starting v{Constants.CLIENT_VERSION}...");

        WindowWidth = ClientSize.X;
        WindowHeight = ClientSize.Y;
        WindowAspectRatio = ClientSize.X / (float)ClientSize.Y;

#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        GL.Enable(EnableCap.DepthTest); // Enable depth testing.
        GL.Enable(EnableCap.Multisample); // Enable multisampling.
        GL.Enable(EnableCap.Blend); // Enable blending for transparent textures.
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor(1f, 0f, 1f, 1.0f);

        // Resource initialization.
        GlobalThreadPool.Initialize();
        TextureRegistry.StartTextureRegistration();
        ModLoader.LoadAllMods();
        TextureRegistry.FinishTextureRegistration();
        _shaderManager = new ShaderManager();

        // World initialization.
        GameTime.Initialize();
        _gameWorld = new GameWorld("World1");
        _skybox = new Skybox(false);

        // PlayerEntity initialization.
        _playerEntity = new PlayerEntity(new Vector3(0, 165, 0), 0, 0);
#if DEBUG
        if (ClientConfig.DebugModeConfig.IsPhotoModeEnabled)
            PhotoModeCamera.Create(new Vector3(0, 256, 48), -30, -100);
#endif
        CursorState = CursorState.Grabbed;

        // UI initialization.
        _crosshair = new Crosshair();
        _imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        ImGuiWindowManager.CreateDefaultWindows();

        ClientLoad?.Invoke();
        Logger.Log("Started.");
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        Logger.Log("Shutting down...");
        _shaderManager.Dispose();
        _skybox.Dispose();
        _imGuiController.DestroyDeviceObjects();
        TextureRegistry.BlockArrayTexture.Dispose();
        _playerEntity.Disable();
        ClientUnload?.Invoke();
    }


    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        double deltaTime = args.Time;
        _fixedFrameAccumulator += deltaTime;
        
        DynamicPerformance.Update(deltaTime);
 
        while (_fixedFrameAccumulator >= Constants.FIXED_DELTA_TIME)
        {
            FixedUpdate();
            _fixedFrameAccumulator -= Constants.FIXED_DELTA_TIME;
        }
 
        double fixedAlpha = _fixedFrameAccumulator / Constants.FIXED_DELTA_TIME;
        
        if (deltaTime > Constants.MAX_DELTA_TIME)
        {
            Logger.LogWarning($"Detected large frame hitch ({1f/deltaTime:F2}fps, {deltaTime:F2}s)! Delta time was clamped to {Constants.MAX_DELTA_TIME:F2} seconds.");
            deltaTime = Constants.MAX_DELTA_TIME;
        }
        else if (deltaTime > Constants.DELTA_TIME_SLOW_THRESHOLD)
        {
            Logger.LogWarning($"Detected frame hitch ({deltaTime:F2}s)!");
            deltaTime = Constants.MAX_DELTA_TIME;
        }
        
        GameTime.Update(deltaTime, fixedAlpha);
        Input.Update(KeyboardState, MouseState);

        Update();
    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

#if DEBUG

        // Set the polygon mode to wireframe if the debug setting is enabled.
        GL.PolygonMode(MaterialFace.FrontAndBack, ClientConfig.DebugModeConfig.RenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
#endif

        // Pass all of these matrices to the vertex shaders.
        // We could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.
        Matrix4 cameraViewMatrix = Camera.RenderingCamera.ViewMatrix;

        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        // Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(TEST_CUBE_ROTATION_SPEED * _timeSinceStartup));
        ShaderManager.UpdateViewMatrix(cameraViewMatrix);
        ShaderManager.UpdateProjectionMatrix(Camera.RenderingCamera.ProjectionMatrix);

        DrawWorld();

#if DEBUG
        if (ClientConfig.DebugModeConfig.RenderSkybox)
#endif
            _skybox.Draw();

#if DEBUG
        if (ClientConfig.DebugModeConfig.IsPhotoModeEnabled && GameTime.TotalTime > 1f && DebugStats.ChunksInGenerationQueue == 0 && DebugStats.ChunksInMeshingQueue == 0)
        {
            ScreenshotUtility.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng(ClientConfig.DebugModeConfig.PhotoModeScreenshotPath, "latest", true, true);
            Close();
            return;
        }

        DebugDrawer.Draw();
#endif

        _crosshair.Draw();

        DrawImGui();

        if (Input.KeyboardState.IsKeyPressed(Keys.F2))
            ScreenshotUtility.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng("Screenshots");

        SwapBuffers();
    }


    /// <summary>
    /// Fixed update loop.
    /// Called <see cref="Constants.FIXED_UPDATE_FRAME_FREQUENCY"/> times per second.
    /// </summary>
    private void FixedUpdate()
    {
        GlobalThreadPool.FixedUpdate();
        _gameWorld.FixedUpdate();
    }


    /// <summary>
    /// Regular update loop.
    /// Called every frame.
    /// </summary>
    private void Update()
    {
        UpdateGui();
        
        GlobalThreadPool.Update();
        _gameWorld.Update();
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


    private void DrawWorld()
    {
        ShaderManager.ChunkShader.Use();

        // Enable backface culling.
        GL.Enable(EnableCap.CullFace);
        _gameWorld.Draw();
        GL.Disable(EnableCap.CullFace);
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
        string message = Marshal.PtrToStringAnsi(pMessage, length);
        
        Logger.LogOpenGl($"[{severity} source={source} type={type} id={id}] {message}");

        if (type == DebugType.DebugTypeError)
            throw new Exception(message);
    }
#endif
}