using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Client.Framework.ECS.Components;

public class PlayerRenderer : Component
{
    // For now the player is a 0.8 x 1.8 x 0.8 block cube.
    private static readonly float[] Vertices =
    {
        // Z- face
        -0.4f,  1.8f, -0.4f,
        -0.4f, 0f, -0.4f,
        0.4f, 0f, -0.4f,
        0.4f, 0f, -0.4f,
        0.4f,  1.8f, -0.4f,
        -0.4f,  1.8f, -0.4f,

        // X- face
        -0.4f, 0f,  0.4f,
        -0.4f, 0f, -0.4f,
        -0.4f,  1.8f, -0.4f,
        -0.4f,  1.8f, -0.4f,
        -0.4f,  1.8f,  0.4f,
        -0.4f, 0f,  0.4f,

        // X+ face
        0.4f, 0f, -0.4f,
        0.4f, 0f,  0.4f,
        0.4f,  1.8f,  0.4f,
        0.4f,  1.8f,  0.4f,
        0.4f,  1.8f, -0.4f,
        0.4f, 0f, -0.4f,

        // Z+ face
        -0.4f, 0f,  0.4f,
        -0.4f,  1.8f,  0.4f,
        0.4f,  1.8f,  0.4f,
        0.4f,  1.8f,  0.4f,
        0.4f, 0f,  0.4f,
        -0.4f, 0f,  0.4f,

        // Y+ face
        -0.4f,  1.8f, -0.4f,
        0.4f,  1.8f, -0.4f,
        0.4f,  1.8f,  0.4f,
        0.4f,  1.8f,  0.4f,
        -0.4f,  1.8f,  0.4f,
        -0.4f,  1.8f, -0.4f,

        // Y- face
        -0.4f, 0f, -0.4f,
        -0.4f, 0f,  0.4f,
        0.4f, 0f, -0.4f,
        0.4f, 0f, -0.4f,
        -0.4f, 0f,  0.4f,
        0.4f, 0f,  0.4f
    };
    
    private readonly Player _player;
    private readonly int _vao;
    private readonly Shader _shader;
    
    
    public PlayerRenderer(Player player)
    {
        _player = player;
        _shader = ShaderManager.PassShader;
        _vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }


    protected override void OnRender(double time)
    {
        _shader.Use();
        _shader.SetMatrix4("model", _player.Transform.GetWorldModelMatrix());
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        GL.BindVertexArray(0);
    }
}