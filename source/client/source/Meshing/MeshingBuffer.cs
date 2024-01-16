using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

/// <summary>
/// Buffer in to which meshes are generated.
/// </summary>
public class MeshingBuffer  //TODO: Instead of storing the opaque and transparent data separately, use two MeshingBuffers.
{
    private const int ELEMENTS_PER_VERTEX = 2;
    private const int FACES_PER_BLOCK = 6;
    private const int VERTICES_PER_FACE = 4;
    private const int INDICES_PER_FACE = 6;

    // Since we cull internal faces, the worst case is half of the faces (every other block needs to be meshed).
    private const int MAX_VISIBLE_FACES = Constants.CHUNK_SIDE_LENGTH_CUBED * FACES_PER_BLOCK / 2;
    private const int MAX_VERTICES_PER_CHUNK = MAX_VISIBLE_FACES * VERTICES_PER_FACE;
    private const int MAX_INDICES_PER_CHUNK = MAX_VISIBLE_FACES * INDICES_PER_FACE;
    private const int MAX_VERTEX_DATA_PER_CHUNK = MAX_VERTICES_PER_CHUNK * ELEMENTS_PER_VERTEX;

    /// <summary>
    /// Array of uints containing the vertex data. 2 uints (64 bits) per vertex.
    /// </summary>
    private readonly uint[] _opaqueVertexData = new uint[MAX_VERTEX_DATA_PER_CHUNK]; // ~3.1 MB
    private readonly uint[] _opaqueIndexData = new uint[MAX_INDICES_PER_CHUNK]; // ~2.4 MB
    /// <summary>
    /// Array of uints containing the vertex data. 2 uints (64 bits) per vertex.
    /// </summary>
    private readonly uint[] _transparentVertexData = new uint[MAX_VERTEX_DATA_PER_CHUNK]; // ~3.1 MB
    private readonly uint[] _transparentIndexData = new uint[MAX_INDICES_PER_CHUNK]; // ~2.4 MB

    private int _addedOpaqueVertexDataCount;
    private int _addedOpaqueIndicesCount;
    private int _addedOpaqueFacesCount;
    private int _addedTransparentVertexDataCount;
    private int _addedTransparentIndicesCount;
    private int _addedTransparentFacesCount;


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
    /// <param name="renderType">Render type of the block</param>
    public void AddFace(MeshingDataCache dataCache, Vector3i blockPos, BlockFace face, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel, BlockRenderType renderType)
    {
        // NOTE: Just to be clear, I'm not proud of this unmaintainable mess... - Japsu
        
        // Vertex positions of the face.
        Vector3i vertexPos0;
        Vector3i vertexPos1;
        Vector3i vertexPos2;
        Vector3i vertexPos3;
        
        // Ambient occlusion neighbours for each vertex. Will be removed later, as when lighting is implemented we get free AO.
        // Vertex 0
        Vector3i aoLeft0;
        Vector3i aoRight0;
        Vector3i aoMiddle0;
        // Vertex 1
        Vector3i aoLeft1;
        Vector3i aoRight1;
        Vector3i aoMiddle1;
        // Vertex 2
        Vector3i aoLeft2;
        Vector3i aoRight2;
        Vector3i aoMiddle2;
        // Vertex 3
        Vector3i aoLeft3;
        Vector3i aoRight3;
        Vector3i aoMiddle3;
        
        int normal = (int)face;
        int blockX = blockPos.X;
        int blockY = blockPos.Y;
        int blockZ = blockPos.Z;
        int xPosNeighbour = blockX + 1;
        int yPosNeighbour = blockY + 1;
        int zPosNeighbour = blockZ + 1;
        int xNegNeighbour = blockX + -1;
        int yNegNeighbour = blockY + -1;
        int zNegNeighbour = blockZ + -1;
        switch (face)
        {
            case BlockFace.XPositive:
                vertexPos0 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                vertexPos1 = new Vector3i(xPosNeighbour, blockY, blockZ);
                vertexPos2 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                vertexPos3 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);

                aoLeft0 = new Vector3i(xPosNeighbour, yNegNeighbour, blockZ);
                aoRight0 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                aoMiddle0 = new Vector3i(xPosNeighbour, yNegNeighbour, zPosNeighbour);
                aoLeft1 = new Vector3i(xPosNeighbour, blockY, zNegNeighbour);
                aoRight1 = new Vector3i(xPosNeighbour, yNegNeighbour, blockZ);
                aoMiddle1 = new Vector3i(xPosNeighbour, yNegNeighbour, zNegNeighbour);
                aoLeft2 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                aoRight2 = new Vector3i(xPosNeighbour, blockY, zNegNeighbour);
                aoMiddle2 = new Vector3i(xPosNeighbour, yPosNeighbour, zNegNeighbour);
                aoLeft3 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                aoRight3 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                aoMiddle3 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                break;
            case BlockFace.YPositive:
                vertexPos0 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                vertexPos1 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                vertexPos2 = new Vector3i(blockX, yPosNeighbour, blockZ);
                vertexPos3 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);

                aoLeft0 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                aoRight0 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);
                aoMiddle0 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                aoLeft1 = new Vector3i(blockX, yPosNeighbour, zNegNeighbour);
                aoRight1 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);
                aoMiddle1 = new Vector3i(xPosNeighbour, yPosNeighbour, zNegNeighbour);
                aoLeft2 = new Vector3i(xNegNeighbour, yPosNeighbour, blockZ);
                aoRight2 = new Vector3i(blockX, yPosNeighbour, zNegNeighbour);
                aoMiddle2 = new Vector3i(xNegNeighbour, yPosNeighbour, zNegNeighbour);
                aoLeft3 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);
                aoRight3 = new Vector3i(xNegNeighbour, yPosNeighbour, blockZ);
                aoMiddle3 = new Vector3i(xNegNeighbour, yPosNeighbour, zPosNeighbour);
                break;
            case BlockFace.ZPositive:
                vertexPos0 = new Vector3i(blockX, blockY, zPosNeighbour);
                vertexPos1 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                vertexPos2 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                vertexPos3 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);

                aoLeft0 = new Vector3i(blockX, yNegNeighbour, zPosNeighbour);
                aoRight0 = new Vector3i(xNegNeighbour, blockY, zPosNeighbour);
                aoMiddle0 = new Vector3i(xNegNeighbour, yNegNeighbour, zPosNeighbour);
                aoLeft1 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                aoRight1 = new Vector3i(blockX, yNegNeighbour, zPosNeighbour);
                aoMiddle1 = new Vector3i(xPosNeighbour, yNegNeighbour, zPosNeighbour);
                aoLeft2 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);
                aoRight2 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                aoMiddle2 = new Vector3i(xPosNeighbour, yPosNeighbour, zPosNeighbour);
                aoLeft3 = new Vector3i(xNegNeighbour, blockY, zPosNeighbour);
                aoRight3 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);
                aoMiddle3 = new Vector3i(xNegNeighbour, yPosNeighbour, zPosNeighbour);
                break;
            case BlockFace.XNegative:
                vertexPos0 = new Vector3i(blockX, blockY, blockZ);
                vertexPos1 = new Vector3i(blockX, blockY, zPosNeighbour);
                vertexPos2 = new Vector3i(blockX, yPosNeighbour, zPosNeighbour);
                vertexPos3 = new Vector3i(blockX, yPosNeighbour, blockZ);

                aoLeft0 = new Vector3i(xNegNeighbour, yNegNeighbour, blockZ);
                aoRight0 = new Vector3i(xNegNeighbour, blockY, zNegNeighbour);
                aoMiddle0 = new Vector3i(xNegNeighbour, yNegNeighbour, zNegNeighbour);
                aoLeft1 = new Vector3i(xNegNeighbour, blockY, zPosNeighbour);
                aoRight1 = new Vector3i(xNegNeighbour, yNegNeighbour, blockZ);
                aoMiddle1 = new Vector3i(xNegNeighbour, yNegNeighbour, zPosNeighbour);
                aoLeft2 = new Vector3i(xNegNeighbour, yPosNeighbour, blockZ);
                aoRight2 = new Vector3i(xNegNeighbour, blockY, zPosNeighbour);
                aoMiddle2 = new Vector3i(xNegNeighbour, yPosNeighbour, zPosNeighbour);
                aoLeft3 = new Vector3i(xNegNeighbour, blockY, zNegNeighbour);
                aoRight3 = new Vector3i(xNegNeighbour, yPosNeighbour, blockZ);
                aoMiddle3 = new Vector3i(xNegNeighbour, yPosNeighbour, zNegNeighbour);
                break;
            case BlockFace.YNegative:
                vertexPos0 = new Vector3i(blockX, blockY, blockZ);
                vertexPos1 = new Vector3i(xPosNeighbour, blockY, blockZ);
                vertexPos2 = new Vector3i(xPosNeighbour, blockY, zPosNeighbour);
                vertexPos3 = new Vector3i(blockX, blockY, zPosNeighbour);

                aoLeft0 = new Vector3i(blockX, yNegNeighbour, zNegNeighbour);
                aoRight0 = new Vector3i(xNegNeighbour, yNegNeighbour, blockZ);
                aoMiddle0 = new Vector3i(xNegNeighbour, yNegNeighbour, zNegNeighbour);
                aoLeft1 = new Vector3i(xPosNeighbour, yNegNeighbour, blockZ);
                aoRight1 = new Vector3i(blockX, yNegNeighbour, zNegNeighbour);
                aoMiddle1 = new Vector3i(xPosNeighbour, yNegNeighbour, zNegNeighbour);
                aoLeft2 = new Vector3i(blockX, yNegNeighbour, zPosNeighbour);
                aoRight2 = new Vector3i(xPosNeighbour, yNegNeighbour, blockZ);
                aoMiddle2 = new Vector3i(xPosNeighbour, yNegNeighbour, zPosNeighbour);
                aoLeft3 = new Vector3i(xNegNeighbour, yNegNeighbour, blockZ);
                aoRight3 = new Vector3i(blockX, yNegNeighbour, zPosNeighbour);
                aoMiddle3 = new Vector3i(xNegNeighbour, yNegNeighbour, zPosNeighbour);
                break;
            case BlockFace.ZNegative:
                vertexPos0 = new Vector3i(xPosNeighbour, blockY, blockZ);
                vertexPos1 = new Vector3i(blockX, blockY, blockZ);
                vertexPos2 = new Vector3i(blockX, yPosNeighbour, blockZ);
                vertexPos3 = new Vector3i(xPosNeighbour, yPosNeighbour, blockZ);

                aoLeft0 = new Vector3i(blockX, yNegNeighbour, zNegNeighbour);
                aoRight0 = new Vector3i(xPosNeighbour, blockY, zNegNeighbour);
                aoMiddle0 = new Vector3i(xPosNeighbour, yNegNeighbour, zNegNeighbour);
                aoLeft1 = new Vector3i(xNegNeighbour, blockY, zNegNeighbour);
                aoRight1 = new Vector3i(blockX, yNegNeighbour, zNegNeighbour);
                aoMiddle1 = new Vector3i(xNegNeighbour, yNegNeighbour, zNegNeighbour);
                aoLeft2 = new Vector3i(blockX, yPosNeighbour, zNegNeighbour);
                aoRight2 = new Vector3i(xNegNeighbour, blockY, zNegNeighbour);
                aoMiddle2 = new Vector3i(xNegNeighbour, yPosNeighbour, zNegNeighbour);
                aoLeft3 = new Vector3i(xPosNeighbour, blockY, zNegNeighbour);
                aoRight3 = new Vector3i(blockX, yPosNeighbour, zNegNeighbour);
                aoMiddle3 = new Vector3i(xPosNeighbour, yPosNeighbour, zNegNeighbour);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(face), face, "What face is THAT?!");
        }

#if DEBUG
        if (ClientConfig.DebugModeConfig.EnableAmbientOcclusion)
        {
#endif
            int ao0 = CalculateAoIndex(dataCache, aoLeft0, aoRight0, aoMiddle0);
            int ao1 = CalculateAoIndex(dataCache, aoLeft1, aoRight1, aoMiddle1);
            int ao2 = CalculateAoIndex(dataCache, aoLeft2, aoRight2, aoMiddle2);
            int ao3 = CalculateAoIndex(dataCache, aoLeft3, aoRight3, aoMiddle3);
            AddVertex(vertexPos0.X, vertexPos0.Y, vertexPos0.Z, normal, 0, ao0, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
            AddVertex(vertexPos1.X, vertexPos1.Y, vertexPos1.Z, normal, 1, ao1, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
            AddVertex(vertexPos2.X, vertexPos2.Y, vertexPos2.Z, normal, 2, ao2, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
            AddVertex(vertexPos3.X, vertexPos3.Y, vertexPos3.Z, normal, 3, ao3, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
#if DEBUG
        }
        else
        {
            AddVertex(vertexPos0.X, vertexPos0.Y, vertexPos0.Z, normal, 0, 3, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
            AddVertex(vertexPos1.X, vertexPos1.Y, vertexPos1.Z, normal, 1, 3, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
            AddVertex(vertexPos2.X, vertexPos2.Y, vertexPos2.Z, normal, 2, 3, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
            AddVertex(vertexPos3.X, vertexPos3.Y, vertexPos3.Z, normal, 3, 3, textureIndex, lightColor, lightLevel, skyLightLevel, renderType);
        }
#endif

        AddIndices(renderType);
    }


    private static int CalculateAoIndex(MeshingDataCache dataCache, Vector3i leftPos, Vector3i rightPos, Vector3i cornerPos)
    {
        BlockState left = dataCache.GetData(leftPos.X, leftPos.Y, leftPos.Z);
        BlockState right = dataCache.GetData(rightPos.X, rightPos.Y, rightPos.Z);
        BlockState corner = dataCache.GetData(cornerPos.X, cornerPos.Y, cornerPos.Z);
        if (left.RenderType == BlockRenderType.Opaque && right.RenderType == BlockRenderType.Opaque)    //TODO: Issues may arise...
            return 0;

        return 3 - (left.RenderType == BlockRenderType.Opaque ? 1 : 0) - (right.RenderType == BlockRenderType.Opaque ? 1 : 0) -
               (corner.RenderType == BlockRenderType.Opaque ? 1 : 0);
    }


    private void AddVertex(int vertexPosX, int vertexPosY, int vertexPosZ, int normal, int textureUvIndex, int aoIndex, int textureIndex, Color9 lightColor,
        int lightLevel, int skyLightLevel, BlockRenderType renderType)
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
        int lightColorValue = lightColor.Packed;

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
        switch (renderType)
        {
            case BlockRenderType.Transparent:
                _transparentVertexData[_addedTransparentVertexDataCount] = data1;
                _transparentVertexData[_addedTransparentVertexDataCount + 1] = data2;
                _addedTransparentVertexDataCount += 2;
                break;
            case BlockRenderType.Opaque:
                _opaqueVertexData[_addedOpaqueVertexDataCount] = data1;
                _opaqueVertexData[_addedOpaqueVertexDataCount + 1] = data2;
                _addedOpaqueVertexDataCount += 2;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(renderType), renderType, null);
        }
    }


    private void AddIndices(BlockRenderType renderType)
    {
        switch (renderType)
        {
            case BlockRenderType.Transparent:
                uint tOffset = 4 * (uint)_addedTransparentFacesCount;
                _transparentIndexData[_addedTransparentIndicesCount] = tOffset + 0;
                _transparentIndexData[_addedTransparentIndicesCount + 1] = tOffset + 1;
                _transparentIndexData[_addedTransparentIndicesCount + 2] = tOffset + 2;
                _transparentIndexData[_addedTransparentIndicesCount + 3] = tOffset + 0;
                _transparentIndexData[_addedTransparentIndicesCount + 4] = tOffset + 2;
                _transparentIndexData[_addedTransparentIndicesCount + 5] = tOffset + 3;
                _addedTransparentIndicesCount += 6;
                _addedTransparentFacesCount++;
                break;
            case BlockRenderType.Opaque:
                uint oOffset = 4 * (uint)_addedOpaqueFacesCount;
                _opaqueIndexData[_addedOpaqueIndicesCount] = oOffset + 0;
                _opaqueIndexData[_addedOpaqueIndicesCount + 1] = oOffset + 1;
                _opaqueIndexData[_addedOpaqueIndicesCount + 2] = oOffset + 2;
                _opaqueIndexData[_addedOpaqueIndicesCount + 3] = oOffset + 0;
                _opaqueIndexData[_addedOpaqueIndicesCount + 4] = oOffset + 2;
                _opaqueIndexData[_addedOpaqueIndicesCount + 5] = oOffset + 3;
                _addedOpaqueIndicesCount += 6;
                _addedOpaqueFacesCount++;
                break;
            case BlockRenderType.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(renderType), renderType, null);
        }
    }


    public ChunkMesh CreateMesh(Vector3i chunkPos)
    {
        uint[] opaqueVertexData = new uint[_addedOpaqueVertexDataCount];
        uint[] opaqueIndices = new uint[_addedOpaqueIndicesCount];
        uint[] transparentVertexData = new uint[_addedTransparentVertexDataCount];
        uint[] transparentIndices = new uint[_addedTransparentIndicesCount];
        
        // Copy opaque data.
        Array.Copy(_opaqueVertexData, opaqueVertexData, _addedOpaqueVertexDataCount);
        Array.Copy(_opaqueIndexData, opaqueIndices, _addedOpaqueIndicesCount);
        
        // Copy transparent data.
        Array.Copy(_transparentVertexData, transparentVertexData, _addedTransparentVertexDataCount);
        Array.Copy(_transparentIndexData, transparentIndices, _addedTransparentIndicesCount);

        return new ChunkMesh(chunkPos, opaqueVertexData, opaqueIndices, transparentVertexData, transparentIndices);
    }


    public void Clear()
    {
        Array.Clear(_opaqueVertexData, 0, _opaqueVertexData.Length);
        Array.Clear(_opaqueIndexData, 0, _opaqueIndexData.Length);
        _addedOpaqueVertexDataCount = 0;
        _addedOpaqueIndicesCount = 0;
        _addedOpaqueFacesCount = 0;
        Array.Clear(_transparentVertexData, 0, _transparentVertexData.Length);
        Array.Clear(_transparentIndexData, 0, _transparentIndexData.Length);
        _addedTransparentVertexDataCount = 0;
        _addedTransparentIndicesCount = 0;
        _addedTransparentFacesCount = 0;
    }
}