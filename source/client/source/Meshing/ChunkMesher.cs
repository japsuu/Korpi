using System.Diagnostics;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Registries;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;
using BlockEngine.Client.World.Regions.Chunks.Blocks;
using JetBrains.Profiler.Api;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Meshing;

public class ChunkMesher
{
    private static readonly ThreadLocal<ChunkMesher> ThreadLocal = new(() => new ChunkMesher());
    public static ChunkMesher ThreadLocalInstance => ThreadLocal.Value!;
    
    /// <summary>
    /// Cache in tyo which the data of the chunk currently being meshed is copied into.
    /// Also includes one block wide border extending into the neighbouring chunks.
    /// </summary>
    private readonly MeshingDataCache _meshingDataCache = new();
    
    /// <summary>
    /// Buffer into which the meshing thread writes the mesh data.
    /// </summary>
    private readonly MeshingBuffer _meshingBuffer = new();
    
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


    public ChunkMesh GenerateMesh(Chunk chunk)
    {
        // if (chunk.Position == new Vector3i(0, 3*Constants.CHUNK_SIZE, 0))
        if (chunk.Position == new Vector3i(0, 4*Constants.CHUNK_SIZE, 0))
            MeasureProfiler.StartCollectingData();
        DebugStats.StartChunkMeshing();
        GameWorld.CurrentGameWorld.RegionManager.FillMeshingCache(chunk.Position, _meshingDataCache);
        
        _meshingDataCache.AcquireNeighbourReadLocks();
        
        _meshingBuffer.Clear();
        
        // Mesh the chunk based on the data cache.
        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                     _meshingDataCache.TryGetData(x, y, z, out BlockState blockState);
                    
                    if (!blockState.ShouldRender())
                        continue;
                    
                    AddFaces(blockState, x, y, z);
                }
            }
        }
        
        _meshingDataCache.ReleaseNeighbourReadLocks();
        
        DebugStats.StopChunkMeshing();
        
        ChunkMesh mesh = _meshingBuffer.CreateMesh(chunk.Position);
        
        // if (chunk.Position == new Vector3i(0, 3*Constants.CHUNK_SIZE, 0))
        if (chunk.Position == new Vector3i(0, 4*Constants.CHUNK_SIZE, 0))
            MeasureProfiler.SaveData();

        return mesh;
    }


    private void AddFaces(BlockState blockState, int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Constants.CHUNK_SIZE, "0 <= x < CHUNK_SIZE");
        Debug.Assert(y >= 0 && y < Constants.CHUNK_SIZE, "0 <= y < CHUNK_SIZE");
        Debug.Assert(z >= 0 && z < Constants.CHUNK_SIZE, "0 <= z < CHUNK_SIZE");
        // Iterate over all 6 faces of the block
        for (int face = 0; face < 6; face++)
        {
            Vector3i neighbourOffset = BlockNeighbourOffsets[face];
            if (!_meshingDataCache.TryGetData(x + neighbourOffset.X, y + neighbourOffset.Y, z + neighbourOffset.Z, out BlockState neighbour))
                continue;

            // If the neighbour is opaque, skip this face.
            // If the neighbour is empty or transparent, we need to mesh this face.
            if (neighbour.RenderType == BlockRenderType.Normal)
                continue;

            // Get the texture index of the block face
            ushort textureIndex = GetBlockFaceTextureIndex(blockState, (BlockFace)face);
                        
            // Get the lighting of the block face
            const int lightLevel = Constants.MAX_LIGHT_LEVEL;
            const int skyLightLevel = Constants.MAX_LIGHT_LEVEL;
            Color9 lightColor = Color9.White;
            
            //TODO: Execute the AO calculation here, to avoid visiting the same blocks multiple times.
                        
            // Add the face to the meshing buffer
            Vector3i blockPos = new(x, y, z);
            _meshingBuffer.AddFace(_meshingDataCache, blockPos, (BlockFace)face, textureIndex, lightColor, lightLevel, skyLightLevel);
        }
    }


    private ushort GetBlockFaceTextureIndex(BlockState block, BlockFace normal)
    {
        return BlockRegistry.GetBlock(block.Id).GetFaceTextureId(block, normal);
    }
}