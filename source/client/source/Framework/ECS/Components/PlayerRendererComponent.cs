using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Client.Framework.ECS.Components;

public class PlayerRendererComponent : Component
{
    // For now the playerEntity is a 0.8 x 1.8 x 0.8 block cube.
    private static readonly float[] Vertices =
    {
        // Z- face
        -0.4f,  1.8f, -0.4f,    1f, 1f, 1f,
        -0.4f, 0f, -0.4f,       1f, 1f, 1f,
        0.4f, 0f, -0.4f,        1f, 1f, 1f,
        0.4f, 0f, -0.4f,        1f, 1f, 1f,
        0.4f,  1.8f, -0.4f,     1f, 1f, 1f,
        -0.4f,  1.8f, -0.4f,    1f, 1f, 1f,

        // X- face
        -0.4f, 0f,  0.4f,       1f, 1f, 1f,
        -0.4f, 0f, -0.4f,       1f, 1f, 1f,
        -0.4f,  1.8f, -0.4f,    1f, 1f, 1f,
        -0.4f,  1.8f, -0.4f,    1f, 1f, 1f,
        -0.4f,  1.8f,  0.4f,    1f, 1f, 1f,
        -0.4f, 0f,  0.4f,       1f, 1f, 1f,

        // X+ face
        0.4f, 0f, -0.4f,        1f, 0f, 0f,
        0.4f, 0f,  0.4f,        1f, 0f, 0f,
        0.4f,  1.8f,  0.4f,     1f, 0f, 0f,
        0.4f,  1.8f,  0.4f,     1f, 0f, 0f,
        0.4f,  1.8f, -0.4f,     1f, 0f, 0f,
        0.4f, 0f, -0.4f,        1f, 0f, 0f,

        // Z+ face
        -0.4f, 0f,  0.4f,       1f, 1f, 1f,
        -0.4f,  1.8f,  0.4f,    1f, 1f, 1f,
        0.4f,  1.8f,  0.4f,     1f, 1f, 1f,
        0.4f,  1.8f,  0.4f,     1f, 1f, 1f,
        0.4f, 0f,  0.4f,        1f, 1f, 1f,
        -0.4f, 0f,  0.4f,       1f, 1f, 1f,

        // Y+ face
        -0.4f,  1.8f, -0.4f,    1f, 1f, 1f,
        0.4f,  1.8f, -0.4f,     1f, 1f, 1f,
        0.4f,  1.8f,  0.4f,     1f, 1f, 1f,
        0.4f,  1.8f,  0.4f,     1f, 1f, 1f,
        -0.4f,  1.8f,  0.4f,    1f, 1f, 1f,
        -0.4f,  1.8f, -0.4f,    1f, 1f, 1f,

        // Y- face
        -0.4f, 0f, -0.4f,       1f, 1f, 1f,
        -0.4f, 0f,  0.4f,       1f, 1f, 1f,
        0.4f, 0f, -0.4f,        1f, 1f, 1f,
        0.4f, 0f, -0.4f,        1f, 1f, 1f,
        -0.4f, 0f,  0.4f,       1f, 1f, 1f,
        0.4f, 0f,  0.4f,        1f, 1f, 1f,
    };
    
    private readonly PlayerEntity _playerEntity;
    private readonly int _vao;
    private readonly Shader _shader;
    
    
    public PlayerRendererComponent(PlayerEntity playerEntity)
    {
        _playerEntity = playerEntity;
        _shader = ShaderManager.PassShader;
        _vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }


    protected override void OnDraw()
    {
        _shader.Use();
        _shader.SetMatrix4("model", _playerEntity.Transform.GetModelMatrix());
        GL.BindVertexArray(_vao);
        GL.CullFace(CullFaceMode.Front);
        GL.Enable(EnableCap.CullFace);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        GL.Disable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);
        GL.BindVertexArray(0);
    }
}