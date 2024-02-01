using System.Diagnostics;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

public class ChunkMesher
{
    private static readonly ThreadLocal<ChunkMesher> ThreadLocal = new(() => new ChunkMesher());
    public static ChunkMesher ThreadLocalInstance => ThreadLocal.Value!;
    
    /// <summary>
    /// Cache in to which the data of the chunk currently being meshed is copied into.
    /// Also includes one block wide border extending into the neighbouring chunks.
    /// </summary>
    private readonly MeshingDataCache _meshingDataCache;
    
    /// <summary>
    /// Buffer to write opaque mesh data into.
    /// </summary>
    private readonly MeshingBuffer _opaqueBuffer;
    
    /// <summary>
    /// Buffer to write transparent mesh data into.
    /// </summary>
    private readonly MeshingBuffer _transparentBuffer;
    
    /// <summary>
    /// Offsets of the 6 neighbours of a block.
    /// Used to quickly fetch the block state of a neighbour based on the face normal.
    /// </summary>
    private static readonly Vector3i[] BlockNeighbourOffsets =
    {
        new(1, 0, 0),
        new(0, 1, 0),
        new(0, 0, 1),
        new(-1, 0, 0),
        new(0, -1, 0),
        new(0, 0, -1),
    };


    private ChunkMesher()
    {
        _meshingDataCache = new MeshingDataCache();
        _opaqueBuffer = new OpaqueMeshingBuffer();
        _transparentBuffer = new TransparentMeshingBuffer();
    }


    public ChunkMesh GenerateMesh(Chunk chunk)
    {
        GameWorld.CurrentGameWorld.ChunkManager.FillMeshingCache(chunk.Position, _meshingDataCache);
        
        //REM: _meshingDataCache.AcquireNeighbourReadLocks();
        
        _opaqueBuffer.Initialize();
        _transparentBuffer.Initialize();
        
        // Mesh the chunk based on the data cache.
        for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
        {
            for (int y = 0; y < Constants.CHUNK_SIDE_LENGTH; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
                {
                     _meshingDataCache.TryGetData(x, y, z, out BlockState blockState);
                    
                    if (!blockState.IsRendered)
                        continue;
                    
                    AddFaces(blockState, x, y, z);
                }
            }
        }
        
        //REM: _meshingDataCache.ReleaseNeighbourReadLocks();
        
        ChunkMesh mesh = CreateMesh(chunk.Position);

        return mesh;
    }


    private void AddFaces(BlockState blockState, int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Constants.CHUNK_SIDE_LENGTH, "0 <= x < CHUNK_SIDE_LENGTH");
        Debug.Assert(y >= 0 && y < Constants.CHUNK_SIDE_LENGTH, "0 <= y < CHUNK_SIDE_LENGTH");
        Debug.Assert(z >= 0 && z < Constants.CHUNK_SIDE_LENGTH, "0 <= z < CHUNK_SIDE_LENGTH");
        
        bool isTransparent = blockState.RenderType == BlockRenderType.Transparent;
        MeshingBuffer meshingBuffer = isTransparent ? _transparentBuffer : _opaqueBuffer;
        
        // Iterate over all 6 faces of the block
        for (int face = 0; face < 6; face++)
        {
            Vector3i neighbourOffset = BlockNeighbourOffsets[face];
            if (!_meshingDataCache.TryGetData(x + neighbourOffset.X, y + neighbourOffset.Y, z + neighbourOffset.Z, out BlockState neighbour))
                continue;

            // Determine if this face should be rendered based on the neighbour's render type.
            switch (neighbour.RenderType)
            {
                case BlockRenderType.None:
                    break;
                case BlockRenderType.Opaque:
                    // If the neighbour is opaque, skip this face.
                    continue;
                case BlockRenderType.AlphaClip:
                    break;
                case BlockRenderType.Transparent:
                    // If this block and the neighbour are both transparent and of the same type, skip this face.
                    if (isTransparent && blockState.Id == neighbour.Id)
                        continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(neighbour.RenderType), neighbour.RenderType, null);
            }
            
            // Get the texture index of the block face
            ushort textureIndex = GetBlockFaceTextureIndex(blockState, (BlockFace)face);
                        
            // Get the lighting of the block face
            const int lightLevel = Constants.MAX_LIGHT_LEVEL;
            const int skyLightLevel = Constants.MAX_LIGHT_LEVEL;
            Color9 lightColor = Color9.White;
            
            //TODO: Execute the AO calculation here, to avoid visiting the same blocks multiple times.
                        
            // Add the face to the meshing buffer
            Vector3i blockPos = new(x, y, z);
            meshingBuffer.AddFace(_meshingDataCache, blockPos, (BlockFace)face, textureIndex, lightColor, lightLevel, skyLightLevel, blockState.RenderType);
        }
    }


    private ChunkMesh CreateMesh(Vector3i chunkPosition)
    {
        uint[] opaqueVertexData = new uint[_opaqueBuffer.AddedVertexDataCount];
        uint[] opaqueIndices = new uint[_opaqueBuffer.AddedIndicesCount];
        uint[] transparentVertexData = new uint[_transparentBuffer.AddedVertexDataCount];
        uint[] transparentIndices = new uint[_transparentBuffer.AddedIndicesCount];
        
        // Copy opaque data.
        Array.Copy(_opaqueBuffer.VertexData, opaqueVertexData, _opaqueBuffer.AddedVertexDataCount);
        Array.Copy(_opaqueBuffer.IndexData, opaqueIndices, _opaqueBuffer.AddedIndicesCount);
        
        // Copy transparent data.
        Array.Copy(_transparentBuffer.VertexData, transparentVertexData, _transparentBuffer.AddedVertexDataCount);
        Array.Copy(_transparentBuffer.IndexData, transparentIndices, _transparentBuffer.AddedIndicesCount);

        return new ChunkMesh(chunkPosition, opaqueVertexData, opaqueIndices, transparentVertexData, transparentIndices);
    }


    private ushort GetBlockFaceTextureIndex(BlockState block, BlockFace normal)
    {
        return BlockRegistry.GetBlock(block.Id).GetFaceTextureId(block, normal);
    }
    
    
    public static void Dispose()
    {
        ThreadLocal.Dispose();
    }
}