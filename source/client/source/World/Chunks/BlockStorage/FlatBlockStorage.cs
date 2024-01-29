using Korpi.Client.Blocks;
using Korpi.Client.Configuration;

namespace Korpi.Client.World.Chunks.BlockStorage;

public class FlatBlockStorage : IBlockStorage
{
    private const int CHUNK_SIZE_CUBED = Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH;
    private readonly BlockState[] _blocks = new BlockState[CHUNK_SIZE_CUBED];
    
    public int RenderedBlockCount { get; private set; }
    public int TranslucentBlockCount { get; private set; }
    
    
    public void SetBlock(SubChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        int index = position.Index;

        oldBlock = _blocks[index];
        
        BlockRenderType oldRenderType = oldBlock.RenderType;
        BlockRenderType newRenderType = block.RenderType;

        if (oldRenderType != newRenderType)
        {
            UpdateContainedCount(oldRenderType, newRenderType);
        }

        _blocks[index] = block;
    }


    private void UpdateContainedCount(BlockRenderType oldRenderType, BlockRenderType newRenderType)
    {
        // Update the rendered block count.
        bool wasRendered = oldRenderType != BlockRenderType.None;
        bool willBeRendered = newRenderType != BlockRenderType.None;
        if (wasRendered && !willBeRendered)
            RenderedBlockCount--;
        else if (!wasRendered && willBeRendered)
            RenderedBlockCount++;
            
        // Update the translucent block count.
        bool wasTransparent = oldRenderType == BlockRenderType.Transparent;
        bool willBeTransparent = newRenderType == BlockRenderType.Transparent;
        if (wasTransparent && !willBeTransparent)
            TranslucentBlockCount--;
        else if (!wasTransparent && willBeTransparent)
            TranslucentBlockCount++;
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