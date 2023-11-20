using BlockEngine.Framework.Blocks;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Buffer in to which meshes are generated.
/// </summary>
public class MeshingBuffer
{
    private const int FACES_PER_BLOCK = 6;
    private const int VERTS_PER_FACE = 4;
    private const int INDICES_PER_FACE = 6;
    
    private readonly ushort[] _vertices;
    private readonly ushort[] _indices;
    
    private int vertexCount;
    private int indexCount;
    
    
    public MeshingBuffer(int chunkSize, bool hasInternalFaceCulling)
    {
        int maxBlocksInChunk = chunkSize * chunkSize * chunkSize;
        int maxVisibleFaces = hasInternalFaceCulling ? maxBlocksInChunk * FACES_PER_BLOCK / 2 : maxBlocksInChunk * FACES_PER_BLOCK;
        int maxVertsInChunk = maxVisibleFaces * VERTS_PER_FACE;
        int maxIndicesPerChunk = maxVisibleFaces * INDICES_PER_FACE;
        _vertices = new ushort[maxVertsInChunk];
        _indices = new ushort[maxIndicesPerChunk];
    }


    //public void AddFace


    public ChunkMesh CreateMesh(Vector3i chunkPos)
    {
        // Create new arrays with the correct size, to avoid sending unused data to the GPU.
        ushort[] vertices = new ushort[vertexCount];
        ushort[] indices = new ushort[indexCount];
        
        Array.Copy(_vertices, vertices, vertexCount);
        Array.Copy(_indices, indices, indexCount);
        
        return new ChunkMesh(chunkPos, vertices, indices);
    }


    public void Clear()
    {
        Array.Clear(_vertices, 0, _vertices.Length);
        Array.Clear(_indices, 0, _indices.Length);
    }
}


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
    private readonly ushort[] _vertices;
    private readonly ushort[] _indices;

    public readonly Vector3i ChunkPos;
    // public readonly VertexArrayObject Vao;
    // public readonly VertexBufferObject Vbo;


    public ChunkMesh(Vector3i chunkPos, ushort[] vertices, ushort[] indices)
    {
        ChunkPos = chunkPos;
        _vertices = vertices;
        _indices = indices;
    }


    public void Draw()
    {
        // Vao.Bind();
        // Vbo.Bind();
        // GL.DrawElements(PrimitiveType.Triangles, 0, VertexCount);
        // Vbo.Unbind();
        // Vao.Unbind();
    }
}
