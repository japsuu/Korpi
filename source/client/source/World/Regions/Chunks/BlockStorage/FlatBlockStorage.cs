using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

public class FlatBlockStorage : IBlockStorage
{
    private readonly BlockState[] _blocks = new BlockState[Constants.CHUNK_SIZE_CUBED];
    
    public int RenderedBlockCount { get; private set; }
    
    
    public void SetBlock(int x, int y, int z, BlockState block, out BlockState oldBlock)
    {
        int index = GetIndex(x, y, z);

        oldBlock = _blocks[index];
        
        bool wasRendered = oldBlock.IsRendered;
        bool willBeRendered = block.IsRendered;
        if (wasRendered && !willBeRendered)
            RenderedBlockCount--;
        else if (!wasRendered && willBeRendered)
            RenderedBlockCount++;

        _blocks[index] = block;
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