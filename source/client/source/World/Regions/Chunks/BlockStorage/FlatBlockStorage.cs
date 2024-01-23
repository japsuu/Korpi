using Korpi.Client.Configuration;
using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

public class FlatBlockStorage : IBlockStorage
{
    private readonly BlockState[] _blocks = new BlockState[Constants.CHUNK_SIDE_LENGTH_CUBED];
    
    public int RenderedBlockCount { get; private set; }
    
    
    public void SetBlock(ChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        int index = position.Index;

        oldBlock = _blocks[index];
        
        bool wasRendered = oldBlock.IsRendered;
        bool willBeRendered = block.IsRendered;
        if (wasRendered && !willBeRendered)
            RenderedBlockCount--;
        else if (!wasRendered && willBeRendered)
            RenderedBlockCount++;

        _blocks[index] = block;
    }


    public BlockState GetBlock(ChunkBlockPosition position)
    {
        return _blocks[position.Index];
    }


    public void Clear()
    {
        Array.Clear(_blocks, 0, _blocks.Length);
        RenderedBlockCount = 0;
    }
}