using System.Diagnostics;
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
    /// <param name="neighbourhood">Array of 27 block states, containing the block neighbourhood.</param>
    /// <param name="blockPos">Position of the block in the chunk (0-31 on all axis)</param>
    /// <param name="face">Which face we are adding</param>
    /// <param name="textureIndex">Index to the texture of this face (0-4095)</param>
    /// <param name="lightColor">Color of the light hitting this face</param>
    /// <param name="lightLevel">Amount of light that hits this face (0-31)</param>
    /// <param name="skyLightLevel">Amount of skylight hitting this face (0-31)</param>
    public void AddFace(BlockState[] neighbourhood, Vector3i blockPos, BlockFace face, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        Vector3i vertPos1;
        Vector3i vertPos2;
        Vector3i vertPos3;
        Vector3i vertPos4;
        int ao1;
        int ao2;
        int ao3;
        int ao4;
        int indexLeft;
        int indexRight;
        int indexCorner;
        int normal = (int)face;
        switch (face)
        {
            case BlockFace.XPositive:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                
                indexLeft = 11;
                indexRight = 23;
                indexCorner = 20;
                ao1 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 5;
                indexRight = 11;
                indexCorner = 2;
                ao2 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 17;
                indexRight = 5;
                indexCorner = 8;
                ao3 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 23;
                indexRight = 17;
                indexCorner = 26;
                ao4 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                break;
            case BlockFace.YPositive:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                
                indexLeft = 17;
                indexRight = 25;
                indexCorner = 26;
                ao1 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 7;
                indexRight = 17;
                indexCorner = 8;
                ao2 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 15;
                indexRight = 7;
                indexCorner = 6;
                ao3 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 25;
                indexRight = 15;
                indexCorner = 24;
                ao4 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                break;
            case BlockFace.ZPositive:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                
                indexLeft = 19;
                indexRight = 21;
                indexCorner = 18;
                ao1 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 23;
                indexRight = 19;
                indexCorner = 20;
                ao2 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 25;
                indexRight = 23;
                indexCorner = 26;
                ao3 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 21;
                indexRight = 25;
                indexCorner = 24;
                ao4 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                break;
            case BlockFace.XNegative:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                
                indexLeft = 9;
                indexRight = 3;
                indexCorner = 0;
                ao1 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 21;
                indexRight = 9;
                indexCorner = 18;
                ao2 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 15;
                indexRight = 21;
                indexCorner = 24;
                ao3 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 3;
                indexRight = 15;
                indexCorner = 6;
                ao4 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                break;
            case BlockFace.YNegative:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                
                indexLeft = 1;
                indexRight = 9;
                indexCorner = 0;
                ao1 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 11;
                indexRight = 1;
                indexCorner = 2;
                ao2 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 19;
                indexRight = 11;
                indexCorner = 20;
                ao3 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 9;
                indexRight = 19;
                indexCorner = 18;
                ao4 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                break;
            case BlockFace.ZNegative:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                
                indexLeft = 1;
                indexRight = 5;
                indexCorner = 2;
                ao1 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 3;
                indexRight = 1;
                indexCorner = 0;
                ao2 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 7;
                indexRight = 3;
                indexCorner = 6;
                ao3 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                
                indexLeft = 5;
                indexRight = 7;
                indexCorner = 8;
                ao4 = CalculateAoIndex(neighbourhood[indexLeft], neighbourhood[indexRight], neighbourhood[indexCorner]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(face), face, "What face is THAT?!");
        }
        AddVertex(vertPos1, normal, 0, ao1, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos2, normal, 1, ao2, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos3, normal, 2, ao3, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos4, normal, 3, ao4, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddIndices();
        AddedFacesCount++;
    }
    
    
    private static int CalculateAoIndex(BlockState left, BlockState right, BlockState corner)
    {
        if (left.RenderType == BlockRenderType.Normal && right.RenderType == BlockRenderType.Normal)
            return 0;
        
        return 3 - (left.RenderType == BlockRenderType.Normal ? 1 : 0) - (right.RenderType == BlockRenderType.Normal ? 1 : 0) - (corner.RenderType == BlockRenderType.Normal ? 1 : 0);

        /*if (left.RenderType == BlockRenderType.None || right.RenderType == BlockRenderType.None || corner.RenderType == BlockRenderType.None)
            return 0;
        if (left.RenderType == BlockRenderType.Normal && right.RenderType == BlockRenderType.Normal && corner.RenderType == BlockRenderType.Normal)
            return 3;
        if (left.RenderType == BlockRenderType.Normal && right.RenderType == BlockRenderType.Normal)
            return 2;
        if (left.RenderType == BlockRenderType.Normal && corner.RenderType == BlockRenderType.Normal)
            return 2;
        if (right.RenderType == BlockRenderType.Normal && corner.RenderType == BlockRenderType.Normal)
            return 2;
        if (left.RenderType == BlockRenderType.Normal || right.RenderType == BlockRenderType.Normal || corner.RenderType == BlockRenderType.Normal)
            return 1;
        return 0;*/
    }


    private void AddVertex(Vector3i vertexPos, int normal, int textureUvIndex, int aoIndex, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
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
        
        int positionIndex = (vertexPos.X << 12) | (vertexPos.Y << 6) | vertexPos.Z;
        int lightColorValue = lightColor.Value;
        
        Debug.Assert(positionIndex is >= 0 and <= 133152);
        Debug.Assert(lightColorValue is >= 0 and <= 511);
        Debug.Assert(lightLevel is >= 0 and <= 31);
        Debug.Assert(skyLightLevel is >= 0 and <= 31);
        Debug.Assert(normal is >= 0 and <= 5);
        Debug.Assert(textureUvIndex is >= 0 and <= 3);
        Debug.Assert(aoIndex is >= 0 and <= 3);
        Debug.Assert(textureIndex is >= 0 and <= 4095);

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
        aoIndex             .InjectUnsigned(ref data2, ref bitIndex2, 2);
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