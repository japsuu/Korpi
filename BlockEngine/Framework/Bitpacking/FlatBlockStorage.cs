using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;

namespace BlockEngine.Framework.Bitpacking;

public class FlatBlockStorage : IBlockStorage
{
    private readonly BlockState[] _blocks = new BlockState[Constants.CHUNK_SIZE_CUBED];
    
    
    public void SetBlock(int x, int y, int z, BlockState block)
    {
        _blocks[GetIndex(x, y, z)] = block;
    }


    public BlockState GetBlock(int x, int y, int z)
    {
        BlockState state = _blocks[GetIndex(x, y, z)];
        return state.IsValid ? state : BlockRegistry.Air.GetDefaultState();
    }
    
    
    private int GetIndex(int x, int y, int z)
    {
        // Calculate the index in a way that minimizes cache trashing.
        return x + Constants.CHUNK_SIZE * (y + Constants.CHUNK_SIZE * z);
    }
}