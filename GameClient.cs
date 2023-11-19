using BlockEngine.Framework;
using BlockEngine.Framework.Configuration;
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
    private Shader _blockShader = null!;
    private Shader _skyboxShader = null!;
    private Skybox _skyboxTexture = null!;


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
    
    private readonly float[] _testCubeVertices = {
        0.5f,   0.5f,   0.0f,
        1.0f,   0.0f,   0.0f,  // top right
        
        0.5f,   -0.5f,  0.0f,
        0.0f,   1.0f,   0.0f,  // bottom right
        
        -0.5f,  -0.5f,  0.0f,
        0.0f,   0.0f,   1.0f,  // bottom left
        
        -0.5f,  0.5f,   0.0f,
        1.0f,   1.0f,   0.0f   // top left
    };
    
    private readonly uint[] _testCubeIndices = {
        // Note that indices always start at 0!
        0, 1, 3,   // first triangle
        1, 2, 3    // second triangle
    };
    
    private World _world = null!;
    
    private int _vertexBufferObject;    // A vertex buffer object (VBO) is a memory buffer in the high speed memory of a video card designed to hold information about vertices.
    private int _elementBufferObject;   // An element buffer object (EBO) is a buffer, just like a vertex buffer object, that stores indices that OpenGL uses to decide what vertices to draw.
    private int _testCubeVertexArrayObject;     // A vertex array object (VAO) is an object which contains one or more vertex buffer objects and is designed to store the information for a complete rendered object.
    private int _skyboxVAO;
    private int _skyboxVBO;
    
    private const float TEST_CUBE_ROTATION_SPEED = 30;

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


    public void SwitchCursorState()
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

        // Generate/bind a VBO.
        // All future calls that modify the VBO will be applied to this buffer from this point forward, until another buffer is bound instead.
        // Upload the vertices to the buffer.
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _testCubeVertices.Length * sizeof(float), _testCubeVertices, BufferUsageHint.StaticDraw);
        
        // Generate/bind a VAO -> used to tell OpenGL how to interpret the vertex data we just loaded (per vertex attribute).
        _testCubeVertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_testCubeVertexArrayObject);
        
        // Tell OpenGL that we want to interpret the vertex data as a list of 3 floats (x, y, z) per vertex.
        // Also enable variable 0 in the shader.
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        // Tell OpenGL that we want to interpret the vertex data as a list of 3 floats (r, g, b) per vertex.
        // Also enable variable 1 in the shader.
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        // Generate/bind a EBO.
        // EBO is NOT global -> It's a property of the currently bound VertexArrayObject. Binding an EBO with no VAO is undefined behaviour.
        // This also means that if you bind another VAO, the current ElementArrayBuffer is going to change with it.
        // We also upload data to the EBO the same way as we did with VBOs.
        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _testCubeIndices.Length * sizeof(uint), _testCubeIndices, BufferUsageHint.StaticDraw);
        
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
        
        // Load the block shader.
        // Just like the VBO, this is global -> every function that uses a shader will modify this one until a new one is bound instead.
        _blockShader = new Shader(IoUtils.GetShaderPath("shader_blocks.vert"), IoUtils.GetShaderPath("shader_blocks.frag"));
        _blockShader.Use();
        
        // Load the skybox shader.
        _skyboxShader = new Shader(IoUtils.GetShaderPath("shader_skybox.vert"), IoUtils.GetShaderPath("shader_skybox.frag"));
        _skyboxShader.Use();
        _skyboxShader.SetInt("skybox", 0);
        
        // Initialize the camera.
        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;
        
        ImGuiWindowManager.CreateDefaultWindows();

        Logger.Log("Started.");
        ClientLoad?.Invoke();
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        
        _blockShader.Dispose();
        _skyboxShader.Dispose();
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

        _blockShader.Use();
        
        // Pass all of these matrices to the vertex shader.
        // We could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.

        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        // Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(TEST_CUBE_ROTATION_SPEED * _timeSinceStartup));
        Matrix4 cameraViewMatrix = _camera.GetViewMatrix();
        Matrix4 cameraProjectionMatrix = _camera.GetProjectionMatrix();
        
        DrawWorld(cameraViewMatrix, cameraProjectionMatrix);
        
        DrawSkybox(cameraViewMatrix, cameraProjectionMatrix, false);
        
        DrawImGui();

        SwapBuffers();
    }


    private void DrawWorld(Matrix4 cameraViewMatrix, Matrix4 cameraProjectionMatrix)
    {
        Matrix4 modelMatrix = Matrix4.Identity;
        _blockShader.SetMatrix4("model", modelMatrix);
        _blockShader.SetMatrix4("view", cameraViewMatrix);
        _blockShader.SetMatrix4("projection", cameraProjectionMatrix);
        
        // Bind the VAO.
        GL.BindVertexArray(_testCubeVertexArrayObject);
        // Draw.
        GL.DrawElements(PrimitiveType.Triangles, _testCubeIndices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }


    private void DrawSkybox(Matrix4 cameraViewMatrix, Matrix4 cameraProjectionMatrix, bool rotateOverTime)
    {
        // Draw skybox.
        GL.DepthFunc(DepthFunction.Lequal);  // Change depth function so depth test passes when values are equal to depth buffer's content
        
        _skyboxShader.Use();
        
        cameraViewMatrix = new Matrix4(new Matrix3(cameraViewMatrix)); // Remove translation from the view matrix
        if (rotateOverTime)
        {
            Matrix4 modelMatrix = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(TEST_CUBE_ROTATION_SPEED * Time.TotalTime));
            _skyboxShader.SetMatrix4("view", modelMatrix * cameraViewMatrix);
        }
        else
        {
            _skyboxShader.SetMatrix4("view", cameraViewMatrix);
        }
        _skyboxShader.SetMatrix4("projection", cameraProjectionMatrix);
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
}