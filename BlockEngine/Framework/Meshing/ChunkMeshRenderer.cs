using BlockEngine.Framework.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

public class ChunkMeshRenderer
{
    private readonly int _meshVBO;
    private readonly int _meshEBO;
    private readonly int _meshVAO;
    private readonly Matrix4 _modelMatrix;
    private readonly ChunkMesh _mesh;


    public ChunkMeshRenderer(ChunkMesh mesh)
    {
        _mesh = mesh;
        _modelMatrix = Matrix4.CreateTranslation(_mesh.ChunkPos);
        _meshVBO = GL.GenBuffer();
        _meshVAO = GL.GenVertexArray();
        _meshEBO = GL.GenBuffer();
        GL.BindVertexArray(_meshVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _meshVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _mesh.Vertices.Length * sizeof(uint), _mesh.Vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _meshEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _mesh.Indices.Length * sizeof(uint), _mesh.Indices, BufferUsageHint.StaticDraw);
    }


    public void Draw(Shader chunkShader)
    {
        chunkShader.SetMatrix4("model", _modelMatrix);

        // Bind the VAO.
        GL.BindVertexArray(_meshVAO);

        // Draw.
        GL.DrawElements(PrimitiveType.Triangles, _mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
}