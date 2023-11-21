using BlockEngine.Framework.Bitpacking;
using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
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
    
    /// <summary>
    /// Array of bytes containing the vertex data. 5 bytes per vertex.
    /// </summary>
    private readonly uint[] _vertexData;
    private readonly uint[] _indices;
    
    private uint _addedVertexDataCount;
    private uint _addedIndicesCount;
    private uint _addedFacesCount;
    
    
    public MeshingBuffer(int chunkSize, bool hasInternalFaceCulling)
    {
        int maxBlocksInChunk = chunkSize * chunkSize * chunkSize;
        int maxVisibleFaces = hasInternalFaceCulling ? maxBlocksInChunk * FACES_PER_BLOCK / 2 : maxBlocksInChunk * FACES_PER_BLOCK;
        int maxVertsInChunk = maxVisibleFaces * VERTS_PER_FACE;
        int maxIndicesPerChunk = maxVisibleFaces * INDICES_PER_FACE;    // 589824 with internal face culling and 32 chunk size.
        _vertexData = new uint[maxVertsInChunk * ELEMENTS_PER_VERTEX];
        _indices = new uint[maxIndicesPerChunk];
    }


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
        // Logger.Debug($"Add face at {blockPos} with normal {faceNormal}");
        AddVertex(vertPos1, normal, 0, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos2, normal, 1, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos3, normal, 2, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos4, normal, 3, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddIndices();
        _addedFacesCount++;
    }


    private void AddVertex(Vector3i vertexPos, int normal, int textureUvIndex, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        // PositionIndex    =   0-35936. Calculated with x + Size * (y + Size * z).             = 16 bits.  16 bits.
        // TextureIndex     =   0-4095.                                                         = 12 bits.  28 bits.
        // LightColor       =   0-511.                                                          = 9 bits.   37 bits.
        // LightLevel       =   0-31.                                                           = 5 bits.   42 bits.
        // SkyLightLevel    =   0-31.                                                           = 5 bits.   47 bits.
        // Normal           =   0-5.                                                            = 3 bits.   50 bits.
        // UVIndex          =   0-3. Could be calculated dynamically based on gl_VertexID.      = 2 bits.   52 bits.
        // 12 bits leftover.
        
        //int positionIndex = vertexPos.X + Constants.CHUNK_VERTEX_MAX_POS * (vertexPos.Y + Constants.CHUNK_VERTEX_MAX_POS * vertexPos.Z);
        int positionIndex = (vertexPos.X << 10) | (vertexPos.Y << 5) | vertexPos.Z;
        int lightColorValue = lightColor.Value;

        if (positionIndex < 0 || positionIndex > 35936)
            throw new ArgumentOutOfRangeException(nameof(positionIndex), positionIndex, "Position index out of range");
        
        if (lightColorValue < 0 || lightColorValue > 511)
            throw new ArgumentOutOfRangeException(nameof(lightColorValue), lightColorValue, "Light color value out of range");
        
        if (lightLevel < 0 || lightLevel > 31)
            throw new ArgumentOutOfRangeException(nameof(lightLevel), lightLevel, "Light level out of range");
        
        if (skyLightLevel < 0 || skyLightLevel > 31)
            throw new ArgumentOutOfRangeException(nameof(skyLightLevel), skyLightLevel, "Skylight level out of range");
        
        if (textureIndex < 0 || textureIndex > 4095)
            throw new ArgumentOutOfRangeException(nameof(textureIndex), textureIndex, "Texture index out of range");
        
        if (normal < 0 || normal > 5)
            throw new ArgumentOutOfRangeException(nameof(normal), normal, "Normal out of range");
        
        if (textureUvIndex < 0 || textureUvIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(textureUvIndex), textureUvIndex, "Texture UV index out of range");

        // NOTE: According to the OpenGL spec, vertex data should be 4-byte aligned. This means that since we cannot fit our vertex in 4 bytes, we use the full 8 bytes.
        // Compress all data to two 32-bit uints...
        uint data1 = 0b_00000000_00000000_00000000_00000000;
        uint data2 = 0b_00000000_00000000_00000000_00000000;
        int bitIndex1 = 0;
        int bitIndex2 = 0;
        
        positionIndex       .InjectUnsigned(ref data1, ref bitIndex1, 16);
        lightColorValue     .InjectUnsigned(ref data1, ref bitIndex1, 9);
        lightLevel          .InjectUnsigned(ref data1, ref bitIndex1, 5);
        textureUvIndex      .InjectUnsigned(ref data1, ref bitIndex1, 2);
        textureIndex        .InjectUnsigned(ref data2, ref bitIndex2, 12);
        skyLightLevel       .InjectUnsigned(ref data2, ref bitIndex2, 5);
        normal              .InjectUnsigned(ref data2, ref bitIndex2, 3);
        _vertexData[_addedVertexDataCount] = data1;
        _vertexData[_addedVertexDataCount + 1] = data2;
        _addedVertexDataCount += 2;
    }
    
    
    private void AddIndices()
    {
        uint offset = 4 * _addedFacesCount;
        _indices[_addedIndicesCount] = offset + 0;
        _indices[_addedIndicesCount + 1] = offset + 1;
        _indices[_addedIndicesCount + 2] = offset + 2;
        _indices[_addedIndicesCount + 3] = offset + 0;
        _indices[_addedIndicesCount + 4] = offset + 2;
        _indices[_addedIndicesCount + 5] = offset + 3;
        _addedIndicesCount += 6;
    }


    public ChunkMesh CreateMesh(Vector3i chunkPos)
    {
        // Create new arrays with the correct size, to avoid sending unused data to the GPU.
        uint[] vertices = new uint[_addedVertexDataCount];
        uint[] indices = new uint[_addedIndicesCount];
        Array.Copy(_vertexData, vertices, _addedVertexDataCount);
        Array.Copy(_indices, indices, _addedIndicesCount);
        
        return new ChunkMesh(chunkPos, vertices, indices);
    }


    public void Clear()
    {
        Array.Clear(_vertexData, 0, _vertexData.Length);
        Array.Clear(_indices, 0, _indices.Length);
        _addedVertexDataCount = 0;
        _addedIndicesCount = 0;
        _addedFacesCount = 0;
    }
}