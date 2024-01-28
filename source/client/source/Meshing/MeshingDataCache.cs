using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

/// <summary>
/// Contains the BlockState data of a subChunk, and a border of its neighbouring chunks.
/// </summary>
public class MeshingDataCache
{
    private readonly Dictionary<Vector3i, SubChunk> _neighbourChunks = new();
    private SubChunk _centerSubChunk = null!;


    public void SetCenterChunk(SubChunk subChunk)
    {
        _centerSubChunk = subChunk;
        
        // Get the neighbouring chunks of the center subChunk.
        _neighbourChunks.Clear();
        foreach (Vector3i offset in ChunkOffsets.SubChunkNeighbourOffsets)
        {
            Vector3i position = subChunk.Position + offset;
            if (position.Y < 0 || position.Y >= Constants.CHUNK_HEIGHT_BLOCKS)
                continue;
            SubChunk? neighbour = GameWorld.CurrentGameWorld.ChunkManager.GetSubChunkAt(position);
            if (neighbour == null)
                continue;
            _neighbourChunks.Add(offset, neighbour);
        }
    }


    /// <summary>
    /// If accessing multiple blocks, loop in the order of z, y, x.
    /// This minimizes cache trashing.
    /// </summary>
    /// <param name="blockPosX">SubChunk-relative X position of the block. Valid range is [-1, Constants.SUBCHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosY">SubChunk-relative Y position of the block. Valid range is [-1, Constants.SUBCHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosZ">SubChunk-relative Z position of the block. Valid range is [-1, Constants.SUBCHUNK_SIDE_LENGTH]</param>
    /// <param name="blockState">The BlockState of the block at the given position.</param>
    /// <returns>If the position was in a loaded subChunk.</returns>
    public bool TryGetData(int blockPosX, int blockPosY, int blockPosZ, out BlockState blockState)
    {
        bool xUnderflow = blockPosX < 0;
        bool xOverflow = blockPosX >= Constants.SUBCHUNK_SIDE_LENGTH;
        bool yUnderflow = blockPosY < 0;
        bool yOverflow = blockPosY >= Constants.SUBCHUNK_SIDE_LENGTH;
        bool zUnderflow = blockPosZ < 0;
        bool zOverflow = blockPosZ >= Constants.SUBCHUNK_SIDE_LENGTH;
        
        // If we are in the center subChunk, we can just get the block state from there.
        if (!xUnderflow && !xOverflow &&
            !yUnderflow && !yOverflow &&
            !zUnderflow && !zOverflow)
        {
            blockState = _centerSubChunk.GetBlockState(new SubChunkBlockPosition(blockPosX, blockPosY, blockPosZ));
            return true;
        }

        // Calculate the subChunk the block is in.
        int chunkPosX = 0;
        int chunkPosY = 0;
        int chunkPosZ = 0;
        
        if (xUnderflow)
        {
            chunkPosX = -Constants.SUBCHUNK_SIDE_LENGTH;
            blockPosX = Constants.SUBCHUNK_SIDE_LENGTH - 1;
        }
        else if (xOverflow)
        {
            chunkPosX = Constants.SUBCHUNK_SIDE_LENGTH;
            blockPosX = 0;
        }

        if (yUnderflow)
        {
            chunkPosY = -Constants.SUBCHUNK_SIDE_LENGTH;
            blockPosY = Constants.SUBCHUNK_SIDE_LENGTH - 1;
        }
        else if (yOverflow)
        {
            chunkPosY = Constants.SUBCHUNK_SIDE_LENGTH;
            blockPosY = 0;
        }
        
        if (zUnderflow)
        {
            chunkPosZ = -Constants.SUBCHUNK_SIDE_LENGTH;
            blockPosZ = Constants.SUBCHUNK_SIDE_LENGTH - 1;
        }
        else if (zOverflow)
        {
            chunkPosZ = Constants.SUBCHUNK_SIDE_LENGTH;
            blockPosZ = 0;
        }
        
        Vector3i chunkPosition = new(chunkPosX, chunkPosY, chunkPosZ);
        SubChunkBlockPosition blockPos = new(blockPosX, blockPosY, blockPosZ);
        
        // If the subChunk is loaded, get the block state from there.
        if (_neighbourChunks.TryGetValue(chunkPosition, out SubChunk? chunk))
        {
            blockState = chunk.GetBlockState(blockPos);
            return true;
        }
        
        // If the subChunk is not loaded, return air.
        blockState = BlockRegistry.Air.GetDefaultState();
        return false;
    }


    /// <summary>
    /// If accessing multiple blocks, loop in the order of z, y, x.
    /// This minimizes cache trashing.
    /// </summary>
    /// <param name="blockPosX">SubChunk-relative X position of the block. Valid range is [-1, Constants.SUBCHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosY">SubChunk-relative Y position of the block. Valid range is [-1, Constants.SUBCHUNK_SIDE_LENGTH]</param>
    /// <param name="blockPosZ">SubChunk-relative Z position of the block. Valid range is [-1, Constants.SUBCHUNK_SIDE_LENGTH]</param>
    /// <returns>The BlockState of the block at the given position.</returns>
    public BlockState GetData(int blockPosX, int blockPosY, int blockPosZ)
    {
        TryGetData(blockPosX, blockPosY, blockPosZ, out BlockState blockState);
        return blockState;
    }
    
    
    public void AcquireNeighbourReadLocks()
    {
        bool success = true;
        foreach (SubChunk chunk in _neighbourChunks.Values)
            success &= chunk.ThreadLock.TryEnterReadLock(Constants.JOB_LOCK_TIMEOUT_MS);

        if (!success)
        {
            ReleaseNeighbourReadLocks();
            throw new Exception("Failed to acquire one or more read locks on neighbouring chunks.");
        }
    }
    
    
    public void ReleaseNeighbourReadLocks()
    {
        foreach (SubChunk chunk in _neighbourChunks.Values)
            chunk.ThreadLock.ExitReadLock();
    }


    public void Clear()
    {
        _centerSubChunk = null!;
        _neighbourChunks.Clear();
    }
}