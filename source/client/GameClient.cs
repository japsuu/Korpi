using BlockEngine.Client.Framework;
using BlockEngine.Client.Framework.Configuration;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Modding;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Framework.Rendering;
using BlockEngine.Client.Framework.Rendering.ImGuiWindows;
using BlockEngine.Client.Framework.Rendering.Shaders;
using BlockEngine.Client.Utils;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Client;

public class GameClient : GameWindow
{
    public static event Action? ClientLoad;
    public static event Action? ClientUnload;

    private readonly bool _isPhotomode;
    
    private ImGuiController _imGuiController = null!;
    private ShaderManager _shaderManager = null!;
    private Skybox _skybox = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private Crosshair _crosshair = null!;


    public GameClient(IReadOnlyList<string> args) : base(
        new GameWindowSettings
        {
            UpdateFrequency = Constants.UPDATE_LOOP_FREQUENCY
        },
        new NativeWindowSettings
        {
            Size = (Settings.Client.WindowSettings.WindowWidth, Settings.Client.WindowSettings.WindowHeight),
            Title = $"{Constants.ENGINE_NAME} v{Constants.ENGINE_VERSION}",
            NumberOfSamples = 8
        })
    {
        // Check if args contains the "-photomode" flag
        _isPhotomode = args.Count > 0 && args[0] == "-photomode";
        if (_isPhotomode)
            Logger.Log("Running in photo mode...");
    }
    
    
    protected override void OnLoad()
    {
        base.OnLoad();
        Logger.Log($"Starting v{Constants.ENGINE_VERSION}...");
        
        GL.Enable(EnableCap.DepthTest);     // Enable depth testing.
        GL.Enable(EnableCap.Multisample);   // Enable multisampling.
        GL.Enable(EnableCap.Blend);         // Enable blending for transparent textures.
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        
        // Resource initialization.
        TextureRegistry.StartTextureRegistration();
        ModLoader.LoadAllMods();
        TextureRegistry.FinishTextureRegistration();
        ShaderManager.UpdateWindowSize(ClientSize.X, ClientSize.Y);
        _shaderManager = new ShaderManager();
        
        // World initialization.
        GameTime.Initialize();
        _world = new World("World1");
        _skybox = new Skybox(false);
        
        // Player initialization.
        if (_isPhotomode)
            _camera = new Camera(new Vector3(48, 256, 48), -60, -100, ClientSize.X / (float)ClientSize.Y);
        else
            _camera = new Camera(Vector3.Zero, ClientSize.X / (float)ClientSize.Y);
        CursorState = CursorState.Grabbed;
        _crosshair = new Crosshair();
        
        // UI initialization.
        _imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        ImGuiWindowManager.CreateDefaultWindows();

        ClientLoad?.Invoke();
        Logger.Log("Started.");
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        
        _shaderManager.Dispose();
        _skybox.Dispose();
        TextureRegistry.BlockArrayTexture.Dispose();
        ClientUnload?.Invoke();
    }


    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        Time.Update(args.Time);
        GameTime.Update();
        
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

        _world.Tick(_camera, args.Time);
    }

    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        
        // Set the polygon mode to wireframe if the debug setting is enabled.
        GL.PolygonMode(MaterialFace.FrontAndBack, DebugSettings.RenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
        
        // Pass all of these matrices to the vertex shaders.
        // We could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.
        Matrix4 cameraViewMatrix = _camera.GetViewMatrix();

        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        // Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(TEST_CUBE_ROTATION_SPEED * _timeSinceStartup));
        ShaderManager.UpdateViewMatrix(cameraViewMatrix);
        ShaderManager.UpdateProjectionMatrix(_camera.GetProjectionMatrix());
        
        DrawWorld();

        if (DebugSettings.RenderSkybox)
            _skybox.Draw();
        
        DebugDrawer.Draw();
        
        _crosshair.Draw();

        DrawImGui();

        if (Input.KeyboardState.IsKeyPressed(Keys.F2))
        {
            Screenshotter.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng("Screenshots");
        }

        if (_isPhotomode && Time.TotalTime > 1f)
        {
            Screenshotter.CaptureFrame(ClientSize.X, ClientSize.Y).SaveAsPng("Screenshots", "latest", true);
            Close();
            return;
        }

        SwapBuffers();
    }


    private void DrawWorld()
    {
        ShaderManager.ChunkShader.Use();

        // Enable backface culling.
        GL.Enable(EnableCap.CullFace);
        _world.DrawChunks(_camera.Transform.Position, ShaderManager.ChunkShader);
        GL.Disable(EnableCap.CullFace);
    }


    private void DrawImGui()
    {
        ImGui.ShowMetricsWindow();
        _imGuiController.Render();
        ImGuiController.CheckGlError("End of frame");
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // if (e.Width == ShaderManager.WindowWidth && e.Height == ShaderManager.WindowHeight)
        //     return;

        GL.Viewport(0, 0, e.Width, e.Height);
        
        ShaderManager.UpdateWindowSize(e.Width, e.Height);

        // Tell ImGui of the new window size.
        _imGuiController.WindowResized(ClientSize.X, ClientSize.Y);
        
        // Tell the camera the new aspect ratio.
        _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
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
        if (CursorState == CursorState.Grabbed)
        {
            _camera.IsMouseFirstMove = true;
            CursorState = CursorState.Normal;
            _camera.IsInputEnabled = false;
        }
        else if (CursorState == CursorState.Normal)
        {
            _camera.IsMouseFirstMove = true;
            CursorState = CursorState.Grabbed;
            _camera.IsInputEnabled = true;
        }
    }
}