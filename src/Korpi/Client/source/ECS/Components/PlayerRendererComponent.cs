using Korpi.Client.ECS.Entities;
using Korpi.Client.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.ECS.Components;

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
    
    
    public PlayerRendererComponent(PlayerEntity playerEntity)
    {
        _playerEntity = playerEntity;
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
        ShaderManager.PositionColorShader.Use();
        ShaderManager.PositionColorShader.ModelMat.Set(_playerEntity.Transform.GetModelMatrix());
        ShaderManager.PositionColorShader.ColorModulator.Set(new Vector4(1, 1, 1, 1));
        
        GL.BindVertexArray(_vao);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Front);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        GL.CullFace(CullFaceMode.Back);
        GL.BindVertexArray(0);
    }
}