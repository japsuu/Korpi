using BlockEngine.Client.Framework.Bitpacking;
using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Meshing;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class Chunk
{
    /// <summary>
    /// Represents the state of the chunk.
    /// </summary>
    public enum ChunkGenerationState
    {
        /// <summary>
        /// The chunk has not been generated yet.
        /// </summary>
        Generating = 0,
        
        /// <summary>
        /// The chunk has been generated and is being meshed.
        /// </summary>
        Meshing = 1,
        
        /// <summary>
        /// The chunk has been loaded and is ready to be used.
        /// </summary>
        Ready = 2
    }

    /// <summary>
    /// Represents the state of the chunk mesh.
    /// </summary>
    public enum ChunkMeshState
    {
        /// <summary>
        /// The chunk has not been meshed.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// The chunk has been meshed.
        /// </summary>
        Meshed = 1,
        
        /// <summary>
        /// The chunk has been meshed but requires a remesh.
        /// </summary>
        Dirty = 2
    }
    
    
    private readonly IBlockStorage _blockStorage = new FlatBlockStorage();

    public readonly Vector3i Position;
    public bool ContainsRenderedBlocks { get; private set; }
    public ChunkGenerationState GenerationState { get; set; }
    public ChunkMeshState MeshState { get; set; }


    public Chunk(Vector3i position)
    {
        Position = position;
        ContainsRenderedBlocks = false;
        GenerationState = ChunkGenerationState.Generating;
        MeshState = ChunkMeshState.None;
    }


    public void Tick()
    {
    }


    public void OnGenerated()
    {
        MeshState = ChunkMeshState.None;
        if (ContainsRenderedBlocks)
        {
            GenerationState = ChunkGenerationState.Meshing;
            ChunkMesher.EnqueueChunkForInitialMeshing(Position);
        }
        else
        {
            OnReady();
        }
    }
    
    
    public void OnMeshed()
    {
        MeshState = ChunkMeshState.Meshed;
        
        OnReady();
    }


    private void OnReady()
    {
        GenerationState = ChunkGenerationState.Ready;
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public void SetBlockState(Vector3i position, BlockState block)
    {
        //TODO: If border block, dirty neighbouring chunk(s) too.
        _blockStorage.SetBlock(position.X, position.Y, position.Z, block, out BlockState oldBlock);
        
        // If the chunk has been meshed and a rendered block was changed, mark the chunk mesh as dirty.
        if (GenerationState == ChunkGenerationState.Ready && MeshState != ChunkMeshState.Dirty && (oldBlock.IsRendered || block.IsRendered))
        {
            SetMeshDirty();
        }
        
        ContainsRenderedBlocks = _blockStorage.RenderedBlockCount > 0;
    }


    public void SetMeshDirty()
    {
        MeshState = ChunkMeshState.Dirty;
        ChunkMesher.EnqueueChunkForMeshing(Position);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(int x, int y, int z)
    {
        return _blockStorage.GetBlock(x, y, z);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(Vector3i position)
    {
        return _blockStorage.GetBlock(position.X, position.Y, position.Z);
    }


    public void CacheMeshingData(MeshingDataCache meshingDataCache)
    {   // TODO: Optimize with block copy.
        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    BlockState state = GetBlockState(x, y, z);
                    // Offset by one block in each direction to account for the border
                    meshingDataCache.SetData(x + 1, y + 1, z + 1, state);
                }
            }
        }
    }


    /// <summary>
    /// Used to get a slice of block data from this chunk.
    /// The size of the slice depends on which neighbouring chunk is asking for the data.
    /// Alternative method would be to query the World.GetBlockData method for each block in the cache, but this would result in cache misses.
    /// </summary>
    /// <param name="cache">Cache to fill with data</param>
    /// <param name="myPosition">Position of this chunk relative to the requesting chunk</param>
    public void CacheMeshingData(MeshingDataCache cache, NeighbouringChunkPosition myPosition)
    {
        switch (myPosition)
        {
            // Corners
            case NeighbouringChunkPosition.CornerNorthEastUp:
                cache.SetData(cache.BorderBlockIndex, cache.BorderBlockIndex, cache.BorderBlockIndex, GetBlockState(0, 0, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthEastUp:
                cache.SetData(0, cache.BorderBlockIndex, cache.BorderBlockIndex, GetBlockState(Constants.CHUNK_SIZE_BITMASK, 0, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthWestUp:
                cache.SetData(0, cache.BorderBlockIndex, 0, GetBlockState(Constants.CHUNK_SIZE_BITMASK, 0, Constants.CHUNK_SIZE_BITMASK));
                break;
            case NeighbouringChunkPosition.CornerNorthWestUp:
                cache.SetData(cache.BorderBlockIndex, cache.BorderBlockIndex, 0, GetBlockState(0, 0, Constants.CHUNK_SIZE_BITMASK));
                break;
            case NeighbouringChunkPosition.CornerNorthEastDown:
                cache.SetData(cache.BorderBlockIndex, 0, cache.BorderBlockIndex, GetBlockState(0, Constants.CHUNK_SIZE_BITMASK, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthEastDown:
                cache.SetData(0, 0, cache.BorderBlockIndex, GetBlockState(Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthWestDown:
                cache.SetData(0, 0, 0, GetBlockState(Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK));
                break;
            case NeighbouringChunkPosition.CornerNorthWestDown:
                cache.SetData(cache.BorderBlockIndex, 0, 0, GetBlockState(0, Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK));
                break;
            
            // Edges
            case NeighbouringChunkPosition.EdgeNorthUp:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(cache.BorderBlockIndex, cache.BorderBlockIndex, z + 1, GetBlockState(0, 0, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeEastUp:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, cache.BorderBlockIndex, cache.BorderBlockIndex, GetBlockState(x, 0, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthUp:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(0, cache.BorderBlockIndex, z + 1, GetBlockState(Constants.CHUNK_SIZE_BITMASK, 0, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeWestUp:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, cache.BorderBlockIndex, 0, GetBlockState(x, 0, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            case NeighbouringChunkPosition.EdgeNorthDown:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(cache.BorderBlockIndex, 0, z + 1, GetBlockState(0, Constants.CHUNK_SIZE_BITMASK, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeEastDown:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, 0, cache.BorderBlockIndex, GetBlockState(x, Constants.CHUNK_SIZE_BITMASK, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthDown:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(0, 0, z + 1, GetBlockState(Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeWestDown:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, 0, 0, GetBlockState(x, Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            case NeighbouringChunkPosition.EdgeNorthEast:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(cache.BorderBlockIndex, y + 1, cache.BorderBlockIndex, GetBlockState(0, y, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthEast:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(0, y + 1, cache.BorderBlockIndex, GetBlockState(Constants.CHUNK_SIZE_BITMASK, y, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthWest:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(0, y + 1, 0, GetBlockState(Constants.CHUNK_SIZE_BITMASK, y, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            case NeighbouringChunkPosition.EdgeNorthWest:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(cache.BorderBlockIndex, y + 1, 0, GetBlockState(0, y, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            
            // Faces
            case NeighbouringChunkPosition.FaceNorth:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(cache.BorderBlockIndex, y + 1, z + 1, GetBlockState(0, y, z));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceEast:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(x + 1, y + 1, cache.BorderBlockIndex, GetBlockState(x, y, 0));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceSouth:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(0, y + 1, z + 1, GetBlockState(Constants.CHUNK_SIZE_BITMASK, y, z));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceWest:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(x + 1, y + 1, 0, GetBlockState(x, y, Constants.CHUNK_SIZE_BITMASK));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceUp:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        cache.SetData(x + 1, cache.BorderBlockIndex, z + 1, GetBlockState(x, 0, z));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceDown:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        cache.SetData(x + 1, 0, z + 1, GetBlockState(x, Constants.CHUNK_SIZE_BITMASK, z));
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(myPosition), myPosition, null);
        }
    }
}