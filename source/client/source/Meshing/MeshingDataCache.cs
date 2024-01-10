using Korpi.Client.Configuration;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Regions.Chunks;
using Korpi.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

/// <summary>
/// Contains the BlockState data of a chunk, and a border of its neighbouring chunks.
/// </summary>
public class MeshingDataCache
{
    private readonly Dictionary<Vector3i, Chunk> _neighbourChunks = new();
    private Chunk _centerChunk = null!;


    public void SetCenterChunk(Chunk chunk)
    {
        _centerChunk = chunk;
        
        // Get the neighbouring chunks of the center chunk.
        _neighbourChunks.Clear();
        foreach (Vector3i position in ChunkOffsets.ChunkNeighbourOffsets)
        {
            Vector3i neighbourPosition = chunk.Position + position;
            Chunk? neighbour = GameWorld.CurrentGameWorld.RegionManager.GetChunkAt(neighbourPosition);
            if (neighbour == null)
                continue;
            _neighbourChunks.Add(position, neighbour);
        }
    }


    /// <summary>
    /// If accessing multiple blocks, loop in the order of z, y, x.
    /// This minimizes cache trashing.
    /// </summary>
    /// <param name="blockPosX">Chunk-relative X position of the block. Valid range is [-1, Constants.CHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosY">Chunk-relative Y position of the block. Valid range is [-1, Constants.CHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosZ">Chunk-relative Z position of the block. Valid range is [-1, Constants.CHUNK_SIDE_LENGTH]</param>
    /// <param name="blockState">The BlockState of the block at the given position.</param>
    /// <returns>If the position was in a loaded chunk.</returns>
    public bool TryGetData(int blockPosX, int blockPosY, int blockPosZ, out BlockState blockState)
    {
        bool xUnderflow = blockPosX < 0;
        bool xOverflow = blockPosX >= Constants.CHUNK_SIDE_LENGTH;
        bool yUnderflow = blockPosY < 0;
        bool yOverflow = blockPosY >= Constants.CHUNK_SIDE_LENGTH;
        bool zUnderflow = blockPosZ < 0;
        bool zOverflow = blockPosZ >= Constants.CHUNK_SIDE_LENGTH;
        
        // If we are in the center chunk, we can just get the block state from there.
        if (!xUnderflow && !xOverflow &&
            !yUnderflow && !yOverflow &&
            !zUnderflow && !zOverflow)
        {
            blockState = _centerChunk.GetBlockState(new Vector3i(blockPosX, blockPosY, blockPosZ));
            return true;
        }

        // Calculate the chunk the block is in.
        int chunkPosX = 0;
        int chunkPosY = 0;
        int chunkPosZ = 0;
        
        if (xUnderflow)
        {
            chunkPosX = -Constants.CHUNK_SIDE_LENGTH;
            blockPosX = Constants.CHUNK_SIDE_LENGTH - 1;
        }
        else if (xOverflow)
        {
            chunkPosX = Constants.CHUNK_SIDE_LENGTH;
            blockPosX = 0;
        }

        if (yUnderflow)
        {
            chunkPosY = -Constants.CHUNK_SIDE_LENGTH;
            blockPosY = Constants.CHUNK_SIDE_LENGTH - 1;
        }
        else if (yOverflow)
        {
            chunkPosY = Constants.CHUNK_SIDE_LENGTH;
            blockPosY = 0;
        }
        
        if (zUnderflow)
        {
            chunkPosZ = -Constants.CHUNK_SIDE_LENGTH;
            blockPosZ = Constants.CHUNK_SIDE_LENGTH - 1;
        }
        else if (zOverflow)
        {
            chunkPosZ = Constants.CHUNK_SIDE_LENGTH;
            blockPosZ = 0;
        }
        
        Vector3i chunkPosition = new(chunkPosX, chunkPosY, chunkPosZ);
        Vector3i blockPos = new(blockPosX, blockPosY, blockPosZ);
        
        // If the chunk is loaded, get the block state from there.
        if (_neighbourChunks.TryGetValue(chunkPosition, out Chunk? chunk))
        {
            blockState = chunk.GetBlockState(blockPos);
            return true;
        }
        
        // If the chunk is not loaded, return air.
        blockState = BlockRegistry.Air.GetDefaultState();
        return false;
    }


    /// <summary>
    /// If accessing multiple blocks, loop in the order of z, y, x.
    /// This minimizes cache trashing.
    /// </summary>
    /// <param name="blockPosX">Chunk-relative X position of the block. Valid range is [-1, Constants.CHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosY">Chunk-relative Y position of the block. Valid range is [-1, Constants.CHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosZ">Chunk-relative Z position of the block. Valid range is [-1, Constants.CHUNK_SIDE_LENGTH]</param>
    /// <returns>The BlockState of the block at the given position.</returns>
    public BlockState GetData(int blockPosX, int blockPosY, int blockPosZ)
    {
        TryGetData(blockPosX, blockPosY, blockPosZ, out BlockState blockState);
        return blockState;
    }
    
    
    public void AcquireNeighbourReadLocks()
    {
        bool success = true;
        foreach (Chunk chunk in _neighbourChunks.Values)
            success &= chunk.ThreadLock.TryEnterReadLock(Constants.JOB_LOCK_TIMEOUT_MS);

        if (!success)
        {
            ReleaseNeighbourReadLocks();
            throw new Exception("Failed to acquire one or more read locks on neighbouring chunks.");
        }
    }
    
    
    public void ReleaseNeighbourReadLocks()
    {
        foreach (Chunk chunk in _neighbourChunks.Values)
            chunk.ThreadLock.ExitReadLock();
    }


    public void Clear()
    {
        _centerChunk = null!;
        _neighbourChunks.Clear();
    }
}