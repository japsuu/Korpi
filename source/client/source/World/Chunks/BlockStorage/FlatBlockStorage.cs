using Korpi.Client.Blocks;
using Korpi.Client.Configuration;

namespace Korpi.Client.World.Chunks.BlockStorage;

public class FlatBlockStorage : IBlockStorage
{
    private const int CHUNK_SIZE_CUBED = Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH;
    private readonly BlockState[] _blocks = new BlockState[CHUNK_SIZE_CUBED];
    
    public int RenderedBlockCount { get; private set; }
    
    
    public void SetBlock(SubChunkBlockPosition position, BlockState block, out BlockState oldBlock)
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


    public BlockState GetBlock(SubChunkBlockPosition position)
    {
        return _blocks[position.Index];
    }


    public void Clear()
    {
        Array.Clear(_blocks, 0, _blocks.Length);
        RenderedBlockCount = 0;
    }
}