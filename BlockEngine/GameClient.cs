using BlockEngine.Framework;
using BlockEngine.Framework.Blocks.Serialization;
using BlockEngine.Framework.Configuration;
using BlockEngine.Framework.Debugging;
using BlockEngine.Framework.Modding;
using BlockEngine.Framework.Registries;
using BlockEngine.Framework.Rendering;
using BlockEngine.Framework.Rendering.ImGuiWindows;
using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Framework.Rendering.Textures;
using BlockEngine.Utils;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine;

public class GameClient : GameWindow
{
    public static event Action? ClientLoad;
    public static event Action? ClientUnload;
    
    private ImGuiController _imGuiController = null!;
    private Texture _testTexture = null!;
    private ShaderManager _shaderManager = null!;
    private Skybox _skybox = null!;
    private World _world = null!;
    private Camera _camera = null!;


    public GameClient() : base(
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
    { }
    
    
    protected override void OnLoad()
    {
        base.OnLoad();
        Logger.Log($"Starting v{Constants.ENGINE_VERSION}...");
        
        _world = new World("World1");
        _imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        
        // We enable depth testing here. If you try to draw something more complex than one plane without this,
        // you'll notice that polygons further in the background will occasionally be drawn over the top of the ones in the foreground.
        GL.Enable(EnableCap.DepthTest);
        
        // Enable multisampling.
        GL.Enable(EnableCap.Multisample);
        
        _testTexture = Texture.LoadFromFile(IoUtils.GetBlockTexturePath("missing.png"));

        TextureRegistry.StartTextureRegistration();
        ModLoader.LoadAllMods();
        TextureRegistry.FinishTextureRegistration();
        
        // Load shaders.
        _shaderManager = new ShaderManager();
        
        _skybox = new Skybox();
        
        // Initialize the camera.
        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;
        
        ImGuiWindowManager.CreateDefaultWindows();
        ShaderManager.UpdateWindowSize(Size.X, Size.Y);

        ClientLoad?.Invoke();
        Logger.Log("Started.");
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        
        _shaderManager.Dispose();
        ClientUnload?.Invoke();
    }


    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        Time.Update(args.Time);
        
        Input.Update(KeyboardState, MouseState);

        // Emulate the cursor being fixed to center of the screen, as OpenTK doesn't fix the cursor position when it's grabbed.
        Vector2 mousePos = Input.MouseState.Position;
        if (CursorState == CursorState.Grabbed)
            mousePos = new Vector2(Size.X / 2f, Size.Y / 2f);
        // MousePosition = new Vector2(Size.X / 2f, Size.Y / 2f);

        _imGuiController.Update(this, (float)args.Time, mousePos);
        
        ImGuiWindowManager.UpdateAllWindows();

        if (Input.KeyboardState.IsKeyPressed(Keys.Escape))
            SwitchCursorState();

        _world.Tick(_camera.Transform.Position, args.Time);
    }

    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        
        // Set the polygon mode to wireframe if the debug setting is enabled.
        GL.PolygonMode(MaterialFace.FrontAndBack, DebugSettings.RenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
        
        // Pass all of these matrices to the vertex shaders.
        // We could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.
        const bool rotateOverTime = false;
        Matrix4 cameraViewMatrix = _camera.GetViewMatrix();
        Matrix4 skyboxViewMatrix = new Matrix4(new Matrix3(cameraViewMatrix)); // Remove translation from the view matrix
        if (rotateOverTime)     //TODO: Move to Skybox.cs
        {
            Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_X * Time.TotalTime)) * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_Y * Time.TotalTime));
            skyboxViewMatrix = modelMatrix * skyboxViewMatrix;
        }

        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        // Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(TEST_CUBE_ROTATION_SPEED * _timeSinceStartup));
        ShaderManager.UpdateViewMatrix(cameraViewMatrix, skyboxViewMatrix);
        ShaderManager.UpdateProjectionMatrix(_camera.GetProjectionMatrix());
        
        DrawWorld();

        if (DebugSettings.RenderSkybox)
            _skybox.Draw();

        DrawImGui();

        SwapBuffers();
    }


    private void DrawWorld()
    {
        ShaderManager.ChunkShader.Use();
        _testTexture.Use(TextureUnit.Texture0);

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

        GL.Viewport(0, 0, e.Width, e.Height);
        
        ShaderManager.UpdateWindowSize(e.Width, e.Height);

        // Tell ImGui of the new window size.
        _imGuiController.WindowResized(ClientSize.X, ClientSize.Y);
        
        // Tell the camera the new aspect ratio.
        _camera.AspectRatio = Size.X / (float)Size.Y;
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