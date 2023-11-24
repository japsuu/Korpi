using BlockEngine.Framework;
using BlockEngine.Framework.Configuration;
using BlockEngine.Framework.Debugging;
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
    private Skybox _skyboxTexture = null!;
    private ShaderManager _shaderManager = null!;


    private readonly float[] _skyboxVertices = {
        // Z- face
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        // X- face
        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        // X+ face
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,

        // Z+ face
        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        // Y+ face
        -1.0f,  1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        // Y- face
        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f,  1.0f
    };
    
    private World _world = null!;
    
    private int _skyboxVAO;
    private int _skyboxVBO;
    
    private const float SKYBOX_ROTATION_SPEED = 30;

    private Camera _camera = null!;
    
    
    public GameClient() : base(new GameWindowSettings 
        {
            UpdateFrequency = Constants.UPDATE_LOOP_FREQUENCY
        }, 
        new NativeWindowSettings
        {
            Size = (Settings.Client.WindowSettings.WindowWidth, Settings.Client.WindowSettings.WindowHeight),
            Title = $"{Constants.ENGINE_NAME} v{Constants.ENGINE_VERSION}"
        }) { }
    
    
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
        
        # region SKYBOX_VAO
        
        _skyboxVAO = GL.GenVertexArray();
        _skyboxVBO = GL.GenBuffer();
        GL.BindVertexArray(_skyboxVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _skyboxVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _skyboxVertices.Length * sizeof(float), _skyboxVertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        # endregion
        
        // Load the skybox texture.
        _skyboxTexture = Skybox.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxTexturePath("x_neg.png"),
            IoUtils.GetSkyboxTexturePath("x_pos.png"),
            IoUtils.GetSkyboxTexturePath("y_neg.png"),
            IoUtils.GetSkyboxTexturePath("y_pos.png"),
            IoUtils.GetSkyboxTexturePath("z_pos.png"),
            IoUtils.GetSkyboxTexturePath("z_neg.png"),
        });
        
        // Load shaders.
        _shaderManager = new ShaderManager();
        
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
        if (rotateOverTime)
        {
            Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(SKYBOX_ROTATION_SPEED * Time.TotalTime));
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
            DrawSkybox();

        DrawImGui();

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


    private void DrawSkybox()
    {
        // Draw skybox.
        GL.DepthFunc(DepthFunction.Lequal);  // Change depth function so depth test passes when values are equal to depth buffer's content
        
        ShaderManager.SkyboxShader.Use();
        
        // Skybox cube...
        _skyboxTexture.Use(TextureUnit.Texture0);
        GL.BindVertexArray(_skyboxVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
        
        GL.DepthFunc(DepthFunction.Less); // set depth function back to default
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