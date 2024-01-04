using System.Runtime.InteropServices;
using BlockEngine.Client.Configuration;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Debugging.Drawing;
using BlockEngine.Client.ECS.Entities;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Modding;
using BlockEngine.Client.Registries;
using BlockEngine.Client.Rendering.Cameras;
using BlockEngine.Client.Rendering.Shaders;
using BlockEngine.Client.Rendering.Skybox;
using BlockEngine.Client.UI;
using BlockEngine.Client.UI.HUD;
using BlockEngine.Client.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Client.Window;

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
    public static bool IsPlayerInGui { get; set; }

    private ImGuiController _imGuiController = null!;
    private ShaderManager _shaderManager = null!;
    private Skybox _skybox = null!;
    private GameWorld _gameWorld = null!;
    private Crosshair _crosshair = null!;
    private PlayerEntity _playerEntity = null!;

#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif


    public GameClient() : base(
        new GameWindowSettings
        {
            UpdateFrequency = Constants.UPDATE_LOOP_FREQUENCY
        },
        new NativeWindowSettings
        {
            Size = (ClientConfig.WindowConfig.WindowWidth, ClientConfig.WindowConfig.WindowHeight),
            Title = $"{Constants.ENGINE_NAME} v{Constants.ENGINE_VERSION}",
            NumberOfSamples = 8,
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        })
    {
    }


    protected override void OnLoad()
    {
        base.OnLoad();
        Logger.Log($"Starting v{Constants.ENGINE_VERSION}...");

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
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        // Resource initialization.
        TextureRegistry.StartTextureRegistration();
        ModLoader.LoadAllMods();
        TextureRegistry.FinishTextureRegistration();
        _shaderManager = new ShaderManager();

        // World initialization.
        GameTime.Initialize();
        _gameWorld = new GameWorld("World1");
        _skybox = new Skybox(false);

        // PlayerEntity initialization.
        _playerEntity = new PlayerEntity(new Vector3(0, 256, 0), 0, 0);
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

        _gameWorld.Dispose();
        _shaderManager.Dispose();
        _skybox.Dispose();
        _imGuiController.DestroyDeviceObjects();
        TextureRegistry.BlockArrayTexture.Dispose();
        ClientUnload?.Invoke();
    }


    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        GameTime.Update(args.Time);

        Input.Update(KeyboardState, MouseState);

        // Emulate the cursor being fixed to center of the screen, as OpenTK doesn't fix the cursor position when it's grabbed.
        Vector2 mousePos = Input.MouseState.Position;
        if (CursorState == CursorState.Grabbed)
            mousePos = new Vector2(ClientSize.X / 2f, ClientSize.Y / 2f);

        // MousePosition = new Vector2(ClientSize.X / 2f, ClientSize.Y / 2f);

        _imGuiController.Update(this, (float)args.Time, mousePos);

        ImGuiWindowManager.UpdateAllWindows();

        if (Input.KeyboardState.IsKeyPressed(Keys.Escape))
            SwitchCursorState();

        _gameWorld.Tick();
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