using Korpi.Client.Bitpacking;
using Korpi.Client.Configuration;

namespace Korpi.Client.World.Chunks;

/// <summary>
/// Keeps track of the highest block in each column of blocks in the chunk.
/// </summary>
public class ChunkHeightmap
{
    private readonly HighPerformanceBitArray _blockmap;  // If a block exists at a given index, the bit is set to true.
    private readonly int[] _heightmap;                      // The height of the highest block in each column. -1 if no blocks exist in the column.


    public ChunkHeightmap()
    {
        _blockmap = new HighPerformanceBitArray(Constants.CHUNK_BLOCKS_COUNT);
        _heightmap = new int[Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH];
        for (int i = 0; i < _heightmap.Length; i++)
            _heightmap[i] = -1;
    }
    
    
    public void OnBlockAdded(int x, int y, int z)
    {
        int blockIndex = GetBlockIndex(x, y, z);
        int columnIndex = GetColumnIndex(x, z);
        _blockmap.Set(blockIndex, true);
        
        if (y > _heightmap[columnIndex])
            _heightmap[columnIndex] = y;
    }
    
    
    public void OnBlockRemoved(int x, int y, int z)
    {
        int blockIndex = GetBlockIndex(x, y, z);
        int columnIndex = GetColumnIndex(x, z);
        _blockmap.Set(blockIndex, false);
        
        if (y == _heightmap[columnIndex])
            _heightmap[columnIndex] = GetHighestBlockInColumn(columnIndex);
    }
    
    
    public int GetHighestBlock(int x, int z)
    {
        int columnIndex = GetColumnIndex(x, z);
        return _heightmap[columnIndex];
    }


    private int GetHighestBlockInColumn(int heightmapIndex)
    {
        for (int y = Constants.SUBCHUNK_SIDE_LENGTH - 1; y >= 0; y--)
        {
            if (_blockmap.Get(y + heightmapIndex * Constants.SUBCHUNK_SIDE_LENGTH))
                return y;
        }
        return -1;
    }


    private static int GetColumnIndex(int x, int z)
    {
        return x + z * Constants.SUBCHUNK_SIDE_LENGTH;
    }


    private static int GetBlockIndex(int x, int y, int z)
    {
        // Calculate the index in such a way, that it is most cache friendly when iterating over y in the innermost loop.
        return y + z * Constants.SUBCHUNK_SIDE_LENGTH + x * Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH;
    }
}