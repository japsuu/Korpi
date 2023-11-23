using System.Diagnostics;
using BlockEngine.Framework.Bitpacking;
using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Buffer in to which meshes are generated.
/// </summary>
public class MeshingBuffer
{
    private const int ELEMENTS_PER_VERTEX = 2;
    private const int FACES_PER_BLOCK = 6;
    private const int VERTS_PER_FACE = 4;
    private const int INDICES_PER_FACE = 6;
    // Since we cull internal faces, the worst case is half of the faces (every other block needs to be meshed).
    private const int MAX_VISIBLE_FACES = Constants.CHUNK_SIZE_CUBED * FACES_PER_BLOCK / 2;
    private const int MAX_VERTS_PER_CHUNK = MAX_VISIBLE_FACES * VERTS_PER_FACE;
    private const int MAX_INDICES_PER_CHUNK = MAX_VISIBLE_FACES * INDICES_PER_FACE;
    private const int MAX_VERTEX_DATA_PER_CHUNK = MAX_VERTS_PER_CHUNK * ELEMENTS_PER_VERTEX;
    
    /// <summary>
    /// Array of bytes containing the vertex data. 2 uints (64 bits) per vertex.
    /// </summary>
    private readonly uint[] _vertexData = new uint[MAX_VERTEX_DATA_PER_CHUNK];
    private readonly uint[] _indices = new uint[MAX_INDICES_PER_CHUNK];
    
    public uint AddedVertexDataCount { get; private set; }
    public uint AddedIndicesCount { get; private set; }
    public uint AddedFacesCount { get; private set; }


    /// <summary>
    /// Adds a block face to the mesh.
    /// </summary>
    /// <param name="blockPos">Position of the block in the chunk (0-31 on all axis)</param>
    /// <param name="faceNormal">Which face we are adding</param>
    /// <param name="textureIndex">Index to the texture of this face (0-4095)</param>
    /// <param name="lightColor">Color of the light hitting this face</param>
    /// <param name="lightLevel">Amount of light that hits this face (0-31)</param>
    /// <param name="skyLightLevel">Amount of skylight hitting this face (0-31)</param>
    public void AddFace(Vector3i blockPos, BlockFaceNormal faceNormal, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        Vector3i vertPos1;
        Vector3i vertPos2;
        Vector3i vertPos3;
        Vector3i vertPos4;
        int normal = (int)faceNormal;
        switch (faceNormal)
        {
            case BlockFaceNormal.XPositive:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                break;
            case BlockFaceNormal.YPositive:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                break;
            case BlockFaceNormal.ZPositive:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                break;
            case BlockFaceNormal.XNegative:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                break;
            case BlockFaceNormal.YNegative:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                break;
            case BlockFaceNormal.ZNegative:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(faceNormal), faceNormal, "What face is THAT?!");
        }
        AddVertex(vertPos1, normal, 0, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos2, normal, 1, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos3, normal, 2, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos4, normal, 3, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddIndices();
        AddedFacesCount++;
    }


    private void AddVertex(Vector3i vertexPos, int normal, int textureUvIndex, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        // PositionIndex    =   0-133152. Calculated with x + Size * (y + Size * z).             = 18 bits.  18 bits.
        // TextureIndex     =   0-4095.                                                         = 12 bits.  30 bits.
        // LightColor       =   0-511.                                                          = 9 bits.   39 bits.
        // LightLevel       =   0-31.                                                           = 5 bits.   44 bits.
        // SkyLightLevel    =   0-31.                                                           = 5 bits.   49 bits.
        // Normal           =   0-5.                                                            = 3 bits.   52 bits.
        // UVIndex          =   0-3. Could be calculated dynamically based on gl_VertexID.      = 2 bits.   54 bits.
        // 10 bits leftover.
        
        int positionIndex = (vertexPos.X << 12) | (vertexPos.Y << 6) | vertexPos.Z;
        int lightColorValue = lightColor.Value;
        
        Debug.Assert(positionIndex >= 0 && positionIndex <= 133152);
        Debug.Assert(lightColorValue >= 0 && lightColorValue <= 511);
        Debug.Assert(lightLevel >= 0 && lightLevel <= 31);
        Debug.Assert(skyLightLevel >= 0 && skyLightLevel <= 31);
        Debug.Assert(normal >= 0 && normal <= 5);
        Debug.Assert(textureUvIndex >= 0 && textureUvIndex <= 3);
        Debug.Assert(textureIndex >= 0 && textureIndex <= 4095);

        // NOTE: According to the OpenGL spec, vertex data should be 4-byte aligned. This means that since we cannot fit our vertex in 4 bytes, we use the full 8 bytes.
        // Compress all data to two 32-bit uints...
        uint data1 = 0b_00000000_00000000_00000000_00000000;
        uint data2 = 0b_00000000_00000000_00000000_00000000;
        int bitIndex1 = 0;
        int bitIndex2 = 0;
        
        positionIndex       .InjectUnsigned(ref data1, ref bitIndex1, 18);
        lightColorValue     .InjectUnsigned(ref data1, ref bitIndex1, 9);
        lightLevel          .InjectUnsigned(ref data1, ref bitIndex1, 5);
        textureIndex        .InjectUnsigned(ref data2, ref bitIndex2, 12);
        skyLightLevel       .InjectUnsigned(ref data2, ref bitIndex2, 5);
        normal              .InjectUnsigned(ref data2, ref bitIndex2, 3);
        textureUvIndex      .InjectUnsigned(ref data2, ref bitIndex2, 2);
        _vertexData[AddedVertexDataCount] = data1;
        _vertexData[AddedVertexDataCount + 1] = data2;
        AddedVertexDataCount += 2;
    }
    
    
    private void AddIndices()
    {
        uint offset = 4 * AddedFacesCount;
        _indices[AddedIndicesCount] = offset + 0;
        _indices[AddedIndicesCount + 1] = offset + 1;
        _indices[AddedIndicesCount + 2] = offset + 2;
        _indices[AddedIndicesCount + 3] = offset + 0;
        _indices[AddedIndicesCount + 4] = offset + 2;
        _indices[AddedIndicesCount + 5] = offset + 3;
        AddedIndicesCount += 6;
    }


    public ChunkRenderer CreateMesh(Vector3i chunkPos)
    {
        return new ChunkRenderer(_vertexData, _indices, chunkPos);
    }


    public void Clear()
    {
        Array.Clear(_vertexData, 0, _vertexData.Length);
        Array.Clear(_indices, 0, _indices.Length);
        AddedVertexDataCount = 0;
        AddedIndicesCount = 0;
        AddedFacesCount = 0;
    }
}