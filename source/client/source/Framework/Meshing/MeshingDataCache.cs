using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Registries;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

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
        foreach (Vector3i position in ChunkHelper.ChunkNeighbourOffsets)
        {
            Vector3i neighbourPosition = chunk.Position + position;
            Chunk? neighbour = World.CurrentWorld.ChunkManager.GetChunkAt(neighbourPosition);
            if (neighbour == null)
                continue;
            _neighbourChunks.Add(position, neighbour);
        }
    }


    /// <summary>
    /// If accessing multiple blocks, loop in the order of z, y, x.
    /// This minimizes cache trashing.
    /// </summary>
    /// <param name="blockPosX">Chunk-relative X position of the block. Valid range is [-1, Constants.CHUNK_SIZE]</param>
    /// <param name="blockPosY">Chunk-relative Y position of the block. Valid range is [-1, Constants.CHUNK_SIZE]</param>
    /// <param name="blockPosZ">Chunk-relative Z position of the block. Valid range is [-1, Constants.CHUNK_SIZE]</param>
    /// <param name="blockState">The BlockState of the block at the given position.</param>
    /// <returns>If the position was in a loaded chunk.</returns>
    public bool TryGetData(int blockPosX, int blockPosY, int blockPosZ, out BlockState blockState)
    {
        bool xUnderflow = blockPosX < 0;
        bool xOverflow = blockPosX >= Constants.CHUNK_SIZE;
        bool yUnderflow = blockPosY < 0;
        bool yOverflow = blockPosY >= Constants.CHUNK_SIZE;
        bool zUnderflow = blockPosZ < 0;
        bool zOverflow = blockPosZ >= Constants.CHUNK_SIZE;
        
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
            chunkPosX = -Constants.CHUNK_SIZE;
            blockPosX = Constants.CHUNK_SIZE - 1;
        }
        else if (xOverflow)
        {
            chunkPosX = Constants.CHUNK_SIZE;
            blockPosX = 0;
        }

        if (yUnderflow)
        {
            chunkPosY = -Constants.CHUNK_SIZE;
            blockPosY = Constants.CHUNK_SIZE - 1;
        }
        else if (yOverflow)
        {
            chunkPosY = Constants.CHUNK_SIZE;
            blockPosY = 0;
        }
        
        if (zUnderflow)
        {
            chunkPosZ = -Constants.CHUNK_SIZE;
            blockPosZ = Constants.CHUNK_SIZE - 1;
        }
        else if (zOverflow)
        {
            chunkPosZ = Constants.CHUNK_SIZE;
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
    /// <param name="blockPosX">Chunk-relative X position of the block. Valid range is [-1, Constants.CHUNK_SIZE]</param>
    /// <param name="blockPosY">Chunk-relative Y position of the block. Valid range is [-1, Constants.CHUNK_SIZE]</param>
    /// <param name="blockPosZ">Chunk-relative Z position of the block. Valid range is [-1, Constants.CHUNK_SIZE]</param>
    /// <returns>The BlockState of the block at the given position.</returns>
    public BlockState GetData(int blockPosX, int blockPosY, int blockPosZ)
    {
        TryGetData(blockPosX, blockPosY, blockPosZ, out BlockState blockState);
        return blockState;
    }


    public void Clear()
    {
        _centerChunk = null!;
        _neighbourChunks.Clear();
    }
}