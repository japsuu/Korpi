using System.Diagnostics;
using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Configuration;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

/// <summary>
/// Buffer in to which meshes are generated.
/// </summary>
public class MeshingBuffer
{
    private const int ELEMENTS_PER_VERTEX = 2;
    private const int FACES_PER_BLOCK = 6;
    private const int VERTICES_PER_FACE = 4;
    private const int INDICES_PER_FACE = 6;

    // Since we cull internal faces, the worst case is half of the faces (every other block needs to be meshed).
    private const int MAX_VISIBLE_FACES = Constants.CHUNK_SIZE_CUBED * FACES_PER_BLOCK / 2;
    private const int MAX_VERTICES_PER_CHUNK = MAX_VISIBLE_FACES * VERTICES_PER_FACE;
    private const int MAX_INDICES_PER_CHUNK = MAX_VISIBLE_FACES * INDICES_PER_FACE;
    private const int MAX_VERTEX_DATA_PER_CHUNK = MAX_VERTICES_PER_CHUNK * ELEMENTS_PER_VERTEX;

    /// <summary>
    /// Array of bytes containing the vertex data. 2 uints (64 bits) per vertex.
    /// </summary>
    private readonly uint[] _vertexData = new uint[MAX_VERTEX_DATA_PER_CHUNK]; // ~3.1 MB
    private readonly uint[] _indexData = new uint[MAX_INDICES_PER_CHUNK]; // ~2.4 MB

    public int AddedVertexDataCount { get; private set; }
    public int AddedIndicesCount { get; private set; }
    public int AddedFacesCount { get; private set; }


    /// <summary>
    /// Adds a block face to the mesh.
    /// </summary>
    /// <param name="dataCache">BlockState data of the chunk being meshed, that can also return data from its neighbouring chunks.</param>
    /// <param name="blockPos">Position of the block in the chunk (0-31 on all axis)</param>
    /// <param name="face">Which face we are adding</param>
    /// <param name="textureIndex">Index to the texture of this face (0-4095)</param>
    /// <param name="lightColor">Color of the light hitting this face</param>
    /// <param name="lightLevel">Amount of light that hits this face (0-31)</param>
    /// <param name="skyLightLevel">Amount of skylight hitting this face (0-31)</param>
    public void AddFace(MeshingDataCache dataCache, Vector3i blockPos, BlockFace face, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        // NOTE: Just to be clear, I'm not proud of this unmaintainable mess... - Japsu
        Vector3i vertexPos0;
        Vector3i vertexPos1;
        Vector3i vertexPos2;
        Vector3i vertexPos3;
        BlockState left0;
        BlockState right0;
        BlockState middle0;
        BlockState left1;
        BlockState right1;
        BlockState middle1;
        BlockState left2;
        BlockState right2;
        BlockState middle2;
        BlockState left3;
        BlockState right3;
        BlockState middle3;
        int normal = (int)face;
        int blockX = blockPos.X;
        int blockY = blockPos.Y;
        int blockZ = blockPos.Z;
        int xPosNeighbour = blockX + 1;
        int yPosNeighbour = blockY + 1;
        int zPosNeighbour = blockZ + 1;
        int xNegNeighbour = blockX + -1;
        int yNegNeighbour = blockY + -1;
        int zNegNeighbour = blockZ + -1;    //TODO: Optimize by merging similar checks and only passing positions to the ao check method.
        switch (face)
        {
            case BlockFace.XPositive:
                vertexPos0 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                vertexPos1 = new Vector3i(xPosNeighbour, blockY, blockZ);
                vertexPos2 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                vertexPos3 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);

                left0 = dataCache.GetData(xPosNeighbour, yNegNeighbour, blockZ);
                right0 = dataCache.GetData(xPosNeighbour, blockY, zPosNeighbour);
                middle0 = dataCache.GetData(xPosNeighbour, yNegNeighbour, zPosNeighbour);
                left1 = dataCache.GetData(xPosNeighbour, blockY, zNegNeighbour);
                right1 = dataCache.GetData(xPosNeighbour, yNegNeighbour, blockZ);
                middle1 = dataCache.GetData(xPosNeighbour, yNegNeighbour, zNegNeighbour);
                left2 = dataCache.GetData(xPosNeighbour, yPosNeighbour, blockZ);
                right2 = dataCache.GetData(xPosNeighbour, blockY, zNegNeighbour);
                middle2 = dataCache.GetData(xPosNeighbour, yPosNeighbour, zNegNeighbour);
                left3 = dataCache.GetData(xPosNeighbour, blockY, zPosNeighbour);
                right3 = dataCache.GetData(xPosNeighbour, yPosNeighbour, blockZ);
                middle3 = dataCache.GetData(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                break;
            case BlockFace.YPositive:
                vertexPos0 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                vertexPos1 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                vertexPos2 = new Vector3i(blockX, yPosNeighbour, blockZ);
                vertexPos3 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);

                left0 = dataCache.GetData(xPosNeighbour, yPosNeighbour, blockZ);
                right0 = dataCache.GetData(blockX, yPosNeighbour, zPosNeighbour);
                middle0 = dataCache.GetData(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                left1 = dataCache.GetData(blockX, yPosNeighbour, zNegNeighbour);
                right1 = dataCache.GetData(xPosNeighbour, yPosNeighbour, blockZ);
                middle1 = dataCache.GetData(xPosNeighbour, yPosNeighbour, zNegNeighbour);
                left2 = dataCache.GetData(xNegNeighbour, yPosNeighbour, blockZ);
                right2 = dataCache.GetData(blockX, yPosNeighbour, zNegNeighbour);
                middle2 = dataCache.GetData(xNegNeighbour, yPosNeighbour, zNegNeighbour);
                left3 = dataCache.GetData(blockX, yPosNeighbour, zPosNeighbour);
                right3 = dataCache.GetData(xNegNeighbour, yPosNeighbour, blockZ);
                middle3 = dataCache.GetData(xNegNeighbour, yPosNeighbour, zPosNeighbour);
                break;
            case BlockFace.ZPositive:
                vertexPos0 = new Vector3i(blockX, blockY, zPosNeighbour);
                vertexPos1 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                vertexPos2 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                vertexPos3 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);

                left0 = dataCache.GetData(blockX, yNegNeighbour, zPosNeighbour);
                right0 = dataCache.GetData(xNegNeighbour, blockY, zPosNeighbour);
                middle0 = dataCache.GetData(xNegNeighbour, yNegNeighbour, zPosNeighbour);
                left1 = dataCache.GetData(xPosNeighbour, blockY, zPosNeighbour);
                right1 = dataCache.GetData(blockX, yNegNeighbour, zPosNeighbour);
                middle1 = dataCache.GetData(xPosNeighbour, yNegNeighbour, zPosNeighbour);
                left2 = dataCache.GetData(blockX, yPosNeighbour, zPosNeighbour);
                right2 = dataCache.GetData(xPosNeighbour, blockY, zPosNeighbour);
                middle2 = dataCache.GetData(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                left3 = dataCache.GetData(xNegNeighbour, blockY, zPosNeighbour);
                right3 = dataCache.GetData(blockX, yPosNeighbour, zPosNeighbour);
                middle3 = dataCache.GetData(xNegNeighbour, yPosNeighbour, zPosNeighbour);
                break;
            case BlockFace.XNegative:
                vertexPos0 = new Vector3i(blockX, blockY, blockZ);
                vertexPos1 = new Vector3i(blockX, blockY, zPosNeighbour);
                vertexPos2 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);
                vertexPos3 = new Vector3i(blockX, yPosNeighbour, blockZ);

                left0 = dataCache.GetData(xNegNeighbour, yNegNeighbour, blockZ);
                right0 = dataCache.GetData(xNegNeighbour, blockY, zNegNeighbour);
                middle0 = dataCache.GetData(xNegNeighbour, yNegNeighbour, zNegNeighbour);
                left1 = dataCache.GetData(xNegNeighbour, blockY, zPosNeighbour);
                right1 = dataCache.GetData(xNegNeighbour, yNegNeighbour, blockZ);
                middle1 = dataCache.GetData(xNegNeighbour, yNegNeighbour, zPosNeighbour);
                left2 = dataCache.GetData(xNegNeighbour, yPosNeighbour, blockZ);
                right2 = dataCache.GetData(xNegNeighbour, blockY, zPosNeighbour);
                middle2 = dataCache.GetData(xNegNeighbour, yPosNeighbour, zPosNeighbour);
                left3 = dataCache.GetData(xNegNeighbour, blockY, zNegNeighbour);
                right3 = dataCache.GetData(xNegNeighbour, yPosNeighbour, blockZ);
                middle3 = dataCache.GetData(xNegNeighbour, yPosNeighbour, zNegNeighbour);
                break;
            case BlockFace.YNegative:
                vertexPos0 = new Vector3i(blockX, blockY, blockZ);
                vertexPos1 = new Vector3i(xPosNeighbour, blockY, blockZ);
                vertexPos2 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                vertexPos3 = new Vector3i(blockX, blockY, zPosNeighbour);

                left0 = dataCache.GetData(blockX, yNegNeighbour, zNegNeighbour);
                right0 = dataCache.GetData(xNegNeighbour, yNegNeighbour, blockZ);
                middle0 = dataCache.GetData(xNegNeighbour, yNegNeighbour, zNegNeighbour);
                left1 = dataCache.GetData(xPosNeighbour, yNegNeighbour, blockZ);
                right1 = dataCache.GetData(blockX, yNegNeighbour, zNegNeighbour);
                middle1 = dataCache.GetData(xPosNeighbour, yNegNeighbour, zNegNeighbour);
                left2 = dataCache.GetData(blockX, yNegNeighbour, zPosNeighbour);
                right2 = dataCache.GetData(xPosNeighbour, yNegNeighbour, blockZ);
                middle2 = dataCache.GetData(xPosNeighbour, yNegNeighbour, zPosNeighbour);
                left3 = dataCache.GetData(xNegNeighbour, yNegNeighbour, blockZ);
                right3 = dataCache.GetData(blockX, yNegNeighbour, zPosNeighbour);
                middle3 = dataCache.GetData(xNegNeighbour, yNegNeighbour, zPosNeighbour);
                break;
            case BlockFace.ZNegative:
                vertexPos0 = new Vector3i(xPosNeighbour, blockY, blockZ);
                vertexPos1 = new Vector3i(blockX, blockY, blockZ);
                vertexPos2 = new Vector3i(blockX, yPosNeighbour, blockZ);
                vertexPos3 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);

                left0 = dataCache.GetData(blockX, yNegNeighbour, zNegNeighbour);
                right0 = dataCache.GetData(xPosNeighbour, blockY, zNegNeighbour);
                middle0 = dataCache.GetData(xPosNeighbour, yNegNeighbour, zNegNeighbour);
                left1 = dataCache.GetData(xNegNeighbour, blockY, zNegNeighbour);
                right1 = dataCache.GetData(blockX, yNegNeighbour, zNegNeighbour);
                middle1 = dataCache.GetData(xNegNeighbour, yNegNeighbour, zNegNeighbour);
                left2 = dataCache.GetData(blockX, yPosNeighbour, zNegNeighbour);
                right2 = dataCache.GetData(xNegNeighbour, blockY, zNegNeighbour);
                middle2 = dataCache.GetData(xNegNeighbour, yPosNeighbour, zNegNeighbour);
                left3 = dataCache.GetData(xPosNeighbour, blockY, zNegNeighbour);
                right3 = dataCache.GetData(blockX, yPosNeighbour, zNegNeighbour);
                middle3 = dataCache.GetData(xPosNeighbour, yPosNeighbour, zNegNeighbour);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(face), face, "What face is THAT?!");
        }

        int ao0 = CalculateAoIndex(left0, right0, middle0);
        int ao1 = CalculateAoIndex(left1, right1, middle1);
        int ao2 = CalculateAoIndex(left2, right2, middle2);
        int ao3 = CalculateAoIndex(left3, right3, middle3);
        AddVertex(vertexPos0.X, vertexPos0.Y, vertexPos0.Z, normal, 0, ao0, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertexPos1.X, vertexPos1.Y, vertexPos1.Z, normal, 1, ao1, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertexPos2.X, vertexPos2.Y, vertexPos2.Z, normal, 2, ao2, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertexPos3.X, vertexPos3.Y, vertexPos3.Z, normal, 3, ao3, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddIndices();
        AddedFacesCount++;
    }


    private static int CalculateAoIndex(BlockState left, BlockState right, BlockState corner)
    {
#if DEBUG
        if (!ClientConfig.DebugModeConfig.EnableAmbientOcclusion)
            return 3;
#endif

        if (left.RenderType == BlockRenderType.Normal && right.RenderType == BlockRenderType.Normal)
            return 0;

        return 3 - (left.RenderType == BlockRenderType.Normal ? 1 : 0) - (right.RenderType == BlockRenderType.Normal ? 1 : 0) -
               (corner.RenderType == BlockRenderType.Normal ? 1 : 0);
    }


    private void AddVertex(int vertexPosX, int vertexPosY, int vertexPosZ, int normal, int textureUvIndex, int aoIndex, int textureIndex, Color9 lightColor,
        int lightLevel, int skyLightLevel)
    {
        // PositionIndex    =   0-133152. Calculated with x + Size * (y + Size * z).             = 18 bits.  18 bits.
        // TextureIndex     =   0-4095.                                                         = 12 bits.  30 bits.
        // LightColor       =   0-511.                                                          = 9 bits.   39 bits.
        // LightLevel       =   0-31.                                                           = 5 bits.   44 bits.
        // SkyLightLevel    =   0-31.                                                           = 5 bits.   49 bits.
        // Normal           =   0-5.                                                            = 3 bits.   52 bits.
        // UVIndex          =   0-3. Could be calculated dynamically based on gl_VertexID.      = 2 bits.   54 bits.
        // AOIndex          =   0-3.                                                            = 2 bits.   56 bits.
        // 8 bits leftover.

        int positionIndex = (vertexPosX << 12) | (vertexPosY << 6) | vertexPosZ;
        int lightColorValue = lightColor.Value;

        Debug.Assert(positionIndex is >= 0 and <= 133152, $"Position index {positionIndex} ({vertexPosX}, {vertexPosY}, {vertexPosZ}) != 0-133152");
        Debug.Assert(lightColorValue is >= 0 and <= 511, $"Light color value {lightColorValue} != 0-511");
        Debug.Assert(lightLevel is >= 0 and <= 31, $"Light level {lightLevel} != 0-31");
        Debug.Assert(skyLightLevel is >= 0 and <= 31, $"Skylight level {skyLightLevel} != 0-31");
        Debug.Assert(normal is >= 0 and <= 5, $"Normal {normal} != 0-5");
        Debug.Assert(textureUvIndex is >= 0 and <= 3, $"Texture UV index {textureUvIndex} != 0-3");
        Debug.Assert(aoIndex is >= 0 and <= 3, $"AO index {aoIndex} != 0-3");
        Debug.Assert(textureIndex is >= 0 and <= 4095, $"Texture index {textureIndex} != 0-4095");

        // NOTE: According to the OpenGL spec, vertex data should be 4-byte aligned. This means that since we cannot fit our vertex in 4 bytes, we use the full 8 bytes.
        // Compress all data to two 32-bit uints...
        uint data1 = 0b_00000000_00000000_00000000_00000000;
        uint data2 = 0b_00000000_00000000_00000000_00000000;
        int bitIndex1 = 0;
        int bitIndex2 = 0;

        data1 |= (uint)positionIndex << bitIndex1;
        bitIndex1 += 18;
        data1 |= (uint)lightColorValue << bitIndex1;
        bitIndex1 += 9;
        data1 |= (uint)lightLevel << bitIndex1;
        data2 |= (uint)textureIndex << bitIndex2;
        bitIndex2 += 12;
        data2 |= (uint)skyLightLevel << bitIndex2;
        bitIndex2 += 5;
        data2 |= (uint)normal << bitIndex2;
        bitIndex2 += 3;
        data2 |= (uint)textureUvIndex << bitIndex2;
        bitIndex2 += 2;
        data2 |= (uint)aoIndex << bitIndex2;
        _vertexData[AddedVertexDataCount] = data1;
        _vertexData[AddedVertexDataCount + 1] = data2;
        AddedVertexDataCount += 2;
    }


    private void AddIndices()
    {
        uint offset = 4 * (uint)AddedFacesCount;
        _indexData[AddedIndicesCount] = offset + 0;
        _indexData[AddedIndicesCount + 1] = offset + 1;
        _indexData[AddedIndicesCount + 2] = offset + 2;
        _indexData[AddedIndicesCount + 3] = offset + 0;
        _indexData[AddedIndicesCount + 4] = offset + 2;
        _indexData[AddedIndicesCount + 5] = offset + 3;
        AddedIndicesCount += 6;
    }


    public ChunkMesh CreateMesh(Vector3i chunkPos)
    {
        uint[] vertexData = new uint[AddedVertexDataCount];
        uint[] indices = new uint[AddedIndicesCount];
        Array.Copy(_vertexData, vertexData, AddedVertexDataCount);
        Array.Copy(_indexData, indices, AddedIndicesCount);

        return new ChunkMesh(chunkPos, vertexData, AddedVertexDataCount, indices, AddedIndicesCount);
    }


    public void Clear()
    {
        Array.Clear(_vertexData, 0, _vertexData.Length);
        Array.Clear(_indexData, 0, _indexData.Length);
        AddedVertexDataCount = 0;
        AddedIndicesCount = 0;
        AddedFacesCount = 0;
    }
}