using BlockEngine.Client.Framework.Blocks;

namespace BlockEngine.Client.Framework.Bitpacking;

public class FlatBlockStorage : IBlockStorage
{
    private readonly BlockState[] _blocks = new BlockState[Constants.CHUNK_SIZE_CUBED];
    
    
    public void SetBlock(int x, int y, int z, BlockState block)
    {
        _blocks[GetIndex(x, y, z)] = block;
    }


    public BlockState GetBlock(int x, int y, int z)
    {
        return _blocks[GetIndex(x, y, z)];
    }
    
    
    private int GetIndex(int x, int y, int z)
    {
        // Calculate the index in a way that minimizes cache trashing.
        return x + Constants.CHUNK_SIZE * (y + Constants.CHUNK_SIZE * z);
    }
}