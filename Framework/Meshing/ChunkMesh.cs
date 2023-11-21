using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Represents the mesh of a single chunk.
/// Contains:
/// - The vertex data. We do not store the indices because we use glDrawArrays.
/// - The chunk position.
/// - The VAO.
/// - The VBO.
/// </summary>
public class ChunkMesh
{
    private readonly uint[] _vertices;
    private readonly uint[] _indices;
    private readonly int _meshVBO;
    private readonly int _meshEBO;
    private readonly int _meshVAO;

    public readonly Vector3i ChunkPos;


    public ChunkMesh(Vector3i chunkPos, uint[] vertices, uint[] indices)
    {
        ChunkPos = chunkPos;
        _vertices = vertices;
        _indices = indices;
        
        _meshVBO = GL.GenBuffer();
        _meshVAO = GL.GenVertexArray();
        GL.BindVertexArray(_meshVAO);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _meshVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(uint), _vertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
        GL.EnableVertexAttribArray(0);
        
        _meshEBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _meshEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        
        Logger.Debug($"Created chunk mesh with {vertices.Length} vertices and {indices.Length} indices.");
        for (int i = 0; i < _vertices.Length; i++)
        {
            Logger.Debug($"Vertex {i} = {_vertices[i]}");
        }
    }


    public void Draw()
    {
        // Bind the VAO.
        GL.BindVertexArray(_meshVAO);
        // Draw.
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
}