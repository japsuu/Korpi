using System.Diagnostics;
using JetBrains.Profiler.Api;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
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
    private readonly MeshingDataCache _meshingDataCache = new();
    
    /// <summary>
    /// Buffers into which the meshing thread writes the mesh data.
    /// One for each LOD level.
    /// </summary>
    private readonly MeshingBuffer[] _meshingBuffers;
    
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


    public ChunkMesher()
    {
        _meshingBuffers = new MeshingBuffer[Constants.TERRAIN_LOD_LEVEL_COUNT];
        for (int i = 0; i < Constants.TERRAIN_LOD_LEVEL_COUNT; i++)
        {
            _meshingBuffers[i] = new MeshingBuffer();
        }
    }


    public LodChunkMesh GenerateMesh(Chunk chunk)
    {
        DebugStats.StartChunkMeshing();
        
        GameWorld.CurrentGameWorld.ChunkManager.FillMeshingCache(chunk.Position, _meshingDataCache);
        
        LodChunkMesh lodMesh = new(chunk.Position);
        
        _meshingDataCache.AcquireNeighbourReadLocks();

        foreach (MeshingBuffer meshingBuffer in _meshingBuffers)
            meshingBuffer.Clear();
        
        // Mesh the chunk based on the data cache.
        // The abomination below is required to avoid showing blocks below the surface.
        bool[] isSurfaceSet = new bool[Constants.TERRAIN_LOD_LEVEL_COUNT - 1];
        for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
        {
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
            {
                bool foundSurfaceBlock = false;
                Array.Clear(isSurfaceSet);
                BlockState surfaceBlock = default;
                for (int y = Constants.CHUNK_SIDE_LENGTH - 1; y >= 0; y--)
                {
                    _meshingDataCache.TryGetData(x, y, z, out BlockState blockState);
                    
                    if (!blockState.IsRendered)
                        continue;
                    
                    if (!foundSurfaceBlock)
                    {
                        surfaceBlock = blockState;
                        foundSurfaceBlock = true;
                    }
                    
                    for (int lodLevel = 0; lodLevel < Constants.TERRAIN_LOD_LEVEL_COUNT; lodLevel++)
                    {
                        // Calculate how many blocks to skip when iterating over the chunk.
                        int velocity = 1 << lodLevel;
                        
                        if (x % velocity != 0 || y % velocity != 0 || z % velocity != 0)
                            continue;

                        if (lodLevel > 0 && !isSurfaceSet[lodLevel - 1])
                        {
                            AddFaces(surfaceBlock, x, y, z, lodLevel, velocity);
                            isSurfaceSet[lodLevel - 1] = true;
                        }
                        else
                        {
                            AddFaces(blockState, x, y, z, lodLevel, velocity);
                        }
                    }
                }
            }
        }
        
        _meshingDataCache.ReleaseNeighbourReadLocks();

        for (int i = 0; i < Constants.TERRAIN_LOD_LEVEL_COUNT; i++)
        {
            ChunkMesh mesh = _meshingBuffers[i].CreateMesh(chunk.Position, i);
            lodMesh.SetMesh(i, mesh);
        }
        
        DebugStats.StopChunkMeshing();

        return lodMesh;
    }


    private void AddFaces(BlockState blockState, int x, int y, int z, int lodLevel, int velocity)
    {
        Debug.Assert(x >= 0 && x < Constants.CHUNK_SIDE_LENGTH, "0 <= x < CHUNK_SIDE_LENGTH");
        Debug.Assert(y >= 0 && y < Constants.CHUNK_SIDE_LENGTH, "0 <= y < CHUNK_SIDE_LENGTH");
        Debug.Assert(z >= 0 && z < Constants.CHUNK_SIDE_LENGTH, "0 <= z < CHUNK_SIDE_LENGTH");
        
        // Iterate over all 6 faces of the block
        for (int face = 0; face < 6; face++)
        {
            Vector3i neighbourOffset = BlockNeighbourOffsets[face] * velocity;
            int neighbourX = x + neighbourOffset.X;
            int neighbourY = y + neighbourOffset.Y;
            int neighbourZ = z + neighbourOffset.Z;
            // For all other LOD levels than the first, force the "side" faces to be rendered. This is to avoid holes in the LOD mesh.
            if (velocity == 1 || (neighbourX is >= 0 and < Constants.CHUNK_SIDE_LENGTH &&
                                  neighbourZ is >= 0 and < Constants.CHUNK_SIDE_LENGTH))
            {
                if (!_meshingDataCache.TryGetData(neighbourX, neighbourY, neighbourZ, out BlockState neighbour))
                    continue;

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
                        if (blockState.RenderType == BlockRenderType.Transparent && blockState.Id == neighbour.Id)
                            continue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(neighbour.RenderType), neighbour.RenderType, null);
                }
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
            _meshingBuffers[lodLevel].AddFace(_meshingDataCache, blockPos, (BlockFace)face, textureIndex, lightColor, lightLevel, skyLightLevel, blockState.RenderType, velocity);
        }
    }


    private ushort GetBlockFaceTextureIndex(BlockState block, BlockFace normal)
    {
        return BlockRegistry.GetBlock(block.Id).GetFaceTextureId(block, normal);
    }
}