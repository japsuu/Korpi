using BlockEngine.Framework.Bitpacking;
using ChunkBenchmark.Bitpackers;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Buffer in to which meshes are generated.
/// </summary>
public class MeshingBuffer
{
    private const int BYTES_PER_VERTEX = 5;
    private const int FACES_PER_BLOCK = 6;
    private const int VERTS_PER_FACE = 4;
    private const int INDICES_PER_FACE = 6;
    
    /// <summary>
    /// Array of bytes containing the vertex data. 5 bytes per vertex.
    /// </summary>
    private readonly byte[] _vertexData;
    private readonly uint[] _indices;
    
    private int _vertexWriteBitPos;
    private int _createdVerticesCount;
    private int _createdIndicesCount;
    
    
    public MeshingBuffer(int chunkSize, bool hasInternalFaceCulling)
    {
        int maxBlocksInChunk = chunkSize * chunkSize * chunkSize;
        int maxVisibleFaces = hasInternalFaceCulling ? maxBlocksInChunk * FACES_PER_BLOCK / 2 : maxBlocksInChunk * FACES_PER_BLOCK;
        int maxVertsInChunk = maxVisibleFaces * VERTS_PER_FACE;
        int maxIndicesPerChunk = maxVisibleFaces * INDICES_PER_FACE;    // 589824 with internal face culling and 32 chunk size.
        _vertexData = new byte[maxVertsInChunk * BYTES_PER_VERTEX];
        _indices = new uint[maxIndicesPerChunk];
    }


    /// <summary>
    /// Adds a block face to the mesh.
    /// </summary>
    /// <param name="blockPos">Position of the block in the chunk (0-31 on all axis)</param>
    /// <param name="faceNormal">Which face we are adding</param>
    /// <param name="textureIndex">Index to the texture of this face (0-4095)</param>
    /// <param name="lighting">Amount of light that hits this face (0-32.</param>
    public void AddFace(Vector3i blockPos, BlockFaceNormal faceNormal, int textureIndex, int lighting)
    {
        int normal = (int)faceNormal;
        switch (faceNormal)
        {
            case BlockFaceNormal.XPositive:
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1), normal, 0, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z), normal, 1, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z), normal, 2, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1), normal, 3, textureIndex, lighting);
                AddIndices();
                break;
            case BlockFaceNormal.XNegative:
                AddVertex(new Vector3i(blockPos.X, blockPos.Y, blockPos.Z), normal, 0, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1), normal, 1, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1), normal, 2, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z), normal, 3, textureIndex, lighting);
                AddIndices();
                break;
            case BlockFaceNormal.YPositive:
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1), normal, 0, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z), normal, 1, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z), normal, 2, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1), normal, 3, textureIndex, lighting);
                AddIndices();
                break;
            case BlockFaceNormal.YNegative:
                AddVertex(new Vector3i(blockPos.X, blockPos.Y, blockPos.Z), normal, 0, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z), normal, 1, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1), normal, 2, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1), normal, 3, textureIndex, lighting);
                AddIndices();
                break;
            case BlockFaceNormal.ZPositive:
                AddVertex(new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1), normal, 0, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1), normal, 1, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1), normal, 2, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1), normal, 3, textureIndex, lighting);
                AddIndices();
                break;
            case BlockFaceNormal.ZNegative:
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z), normal, 0, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y, blockPos.Z), normal, 1, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z), normal, 2, textureIndex, lighting);
                AddVertex(new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z), normal, 3, textureIndex, lighting);
                AddIndices();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(faceNormal), faceNormal, "What normal is THAT?!");
        }
    }


    private unsafe void AddVertex(Vector3i vertexPos, int normal, int textureUVs, int textureIndex, int lighting)
    {
        // Each vertex must fit in 5 bytes (40 bits).
        // Vertex position is packed to 18 bits. Because every vertex pos is in range 0-32, we can pack it to 6 bits per axis.
        // Normal is packed to 3 bits, as there are only 6 possible normals.    21 bits.
        // Texture UVs are packed to 2 bits, as there are only 4 possible UVs.  23 bits.
        // Texture index is packed to 12 bits, as there are max 4096 textures.  35 bits.
        // Lighting is packed to 5 bits, as there are 32 possible light levels. 40 bits.
        
        // Compress all data to a ulong...
        ulong data = 0b_00000000_00000000_00000000_00000000_00000000;
        int bitIndex = 0;
        // Use PrimitiveSerializeExt to write the data to the ulong.
        vertexPos.X.InjectUnsigned(ref data, ref bitIndex, 6);
        vertexPos.Y.InjectUnsigned(ref data, ref bitIndex, 6);
        vertexPos.Z.InjectUnsigned(ref data, ref bitIndex, 6);
        normal.InjectUnsigned(ref data, ref bitIndex, 3);
        textureUVs.InjectUnsigned(ref data, ref bitIndex, 2);
        textureIndex.InjectUnsigned(ref data, ref bitIndex, 12);
        lighting.InjectUnsigned(ref data, ref bitIndex, 5);
        
        // We use ArraySerializeUnsafe to write the data to the array.
        // Pin the array before long sequences of reads or writes.
        fixed (byte* bPtr = _vertexData)
        {
            // Cast the byte* to ulong*
            ulong* uPtr = (ulong*)bPtr;

            ArraySerializeUnsafe.Write(uPtr, data, ref _vertexWriteBitPos, bitIndex);
        }
    }
    
    
    private void AddIndices()
    {
        uint offset = 4 * (uint)_createdVerticesCount;
        _indices[_createdIndicesCount++] = offset + 0;
        _indices[_createdIndicesCount++] = offset + 1;
        _indices[_createdIndicesCount++] = offset + 2;
        _indices[_createdIndicesCount++] = offset + 0;
        _indices[_createdIndicesCount++] = offset + 2;
        _indices[_createdIndicesCount++] = offset + 3;
        
        _createdVerticesCount += 4;
    }


    public ChunkMesh CreateMesh(Vector3i chunkPos)
    {
        // Create new arrays with the correct size, to avoid sending unused data to the GPU.
        byte[] vertices = new byte[_createdVerticesCount * BYTES_PER_VERTEX];
        uint[] indices = new uint[_createdIndicesCount];
        
        Array.Copy(_vertexData, vertices, _createdVerticesCount * BYTES_PER_VERTEX);
        Array.Copy(_indices, indices, _createdIndicesCount);
        
        return new ChunkMesh(chunkPos, vertices, indices);
    }


    public void Clear()
    {
        Array.Clear(_vertexData, 0, _vertexData.Length);
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
    private readonly byte[] _vertices;
    private readonly uint[] _indices;

    public readonly Vector3i ChunkPos;
    // public readonly VertexArrayObject Vao;
    // public readonly VertexBufferObject Vbo;


    public ChunkMesh(Vector3i chunkPos, byte[] vertices, uint[] indices)
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
