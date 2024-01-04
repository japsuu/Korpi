using OpenTK.Mathematics;

namespace BlockEngine.Client.Meshing;

/// <summary>
/// Contains the mesh data for a chunk, and the chunk's position.
/// </summary>
public class ChunkMesh
{
    public readonly Vector3i ChunkPos;
    
    public readonly uint[] VertexData;
    public readonly int VerticesCount;
    
    public readonly uint[] IndexData;
    public readonly int IndicesCount;


    public ChunkMesh(Vector3i chunkPos, uint[] vertexData, int verticesCount, uint[] indexData, int indicesCount)
    {
        ChunkPos = chunkPos;
        VertexData = vertexData;
        VerticesCount = verticesCount;
        IndexData = indexData;
        IndicesCount = indicesCount;
    }
}