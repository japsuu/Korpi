using System.Diagnostics;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

/// <summary>
/// Buffer of memory, into which mesh data is written.
/// </summary>
public abstract class MeshingBuffer : IDisposable 
{
    protected const int ELEMENTS_PER_VERTEX = 2;
    protected const int FACES_PER_BLOCK = 6;
    protected const int VERTICES_PER_FACE = 4;
    protected const int INDICES_PER_FACE = 6;
    protected const int CHUNK_SIZE_CUBED = Constants.CHUNK_SIDE_LENGTH * Constants.CHUNK_SIDE_LENGTH * Constants.CHUNK_SIDE_LENGTH;

    /// <summary>
    /// Array of uints containing the vertex data. 2 uints (64 bits) per vertex.
    /// </summary>
    public readonly uint[] VertexData;    // 6.29 MB

    /// <summary>
    /// Array of uints containing the index data. 1 uint (32 bits) per vertex.
    /// </summary>
    public readonly uint[] IndexData;         // 4.72 MB


    public MeshingBuffer(int maxVertexDataPerChunk, int maxIndicesPerChunk)
    {
        VertexData = new uint[maxVertexDataPerChunk];
        IndexData = new uint[maxIndicesPerChunk];
    }


    /// <summary>
    /// The amount of vertex data currently in the buffer.
    /// </summary>
    public int AddedVertexDataCount { get; private set; }
    
    /// <summary>
    /// The amount of indices currently in the buffer.
    /// </summary>
    public int AddedIndicesCount { get; private set; }
    
    /// <summary>
    /// The amount of faces currently in the buffer.
    /// </summary>
    public int AddedFacesCount { get; private set; }


    /// <summary>
    /// Initializes the buffer for meshing.
    /// Does not clear the buffer, but resets the internal counters.
    /// </summary>
    public void Initialize()
    {
        AddedVertexDataCount = 0;
        AddedIndicesCount = 0;
        AddedFacesCount = 0;
    }


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
            AddVertex(vertexPos0.X, vertexPos0.Y, vertexPos0.Z, normal, 0, ao0, textureIndex, lightColor, lightLevel, skyLightLevel);
            AddVertex(vertexPos1.X, vertexPos1.Y, vertexPos1.Z, normal, 1, ao1, textureIndex, lightColor, lightLevel, skyLightLevel);
            AddVertex(vertexPos2.X, vertexPos2.Y, vertexPos2.Z, normal, 2, ao2, textureIndex, lightColor, lightLevel, skyLightLevel);
            AddVertex(vertexPos3.X, vertexPos3.Y, vertexPos3.Z, normal, 3, ao3, textureIndex, lightColor, lightLevel, skyLightLevel);
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

        // Add indices for the face.
        uint offset = 4 * (uint)AddedFacesCount;
        IndexData[AddedIndicesCount] = offset + 0;
        IndexData[AddedIndicesCount + 1] = offset + 1;
        IndexData[AddedIndicesCount + 2] = offset + 2;
        IndexData[AddedIndicesCount + 3] = offset + 0;
        IndexData[AddedIndicesCount + 4] = offset + 2;
        IndexData[AddedIndicesCount + 5] = offset + 3;
        AddedIndicesCount += 6;
        AddedFacesCount++;
    }


    private static int CalculateAoIndex(MeshingDataCache dataCache, Vector3i leftPos, Vector3i rightPos, Vector3i cornerPos)
    {
        BlockState left = dataCache.GetData(leftPos.X, leftPos.Y, leftPos.Z);
        BlockState right = dataCache.GetData(rightPos.X, rightPos.Y, rightPos.Z);
        BlockState corner = dataCache.GetData(cornerPos.X, cornerPos.Y, cornerPos.Z);
        if (left.RenderType == BlockRenderType.Opaque && right.RenderType == BlockRenderType.Opaque)
            return 0;

        return 3 - (left.RenderType == BlockRenderType.Opaque ? 1 : 0) - (right.RenderType == BlockRenderType.Opaque ? 1 : 0) -
               (corner.RenderType == BlockRenderType.Opaque ? 1 : 0);
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
        
        VertexData[AddedVertexDataCount] = data1;
        VertexData[AddedVertexDataCount + 1] = data2;
        AddedVertexDataCount += 2;
    }


    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~MeshingBuffer()
    {
        ReleaseUnmanagedResources();
    }
}