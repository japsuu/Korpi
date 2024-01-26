using Korpi.Client.Registries;
using Korpi.Client.World.Chunks.Blocks;

namespace Korpi.Client.World.Chunks.BlockStorage;

public class DummyBlockStorage : IBlockStorage
{
    public int RenderedBlockCount => 0;
    
    
    public void SetBlock(SubChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        oldBlock = BlockRegistry.Air.GetDefaultState();
    }


    public BlockState GetBlock(SubChunkBlockPosition position)
    {
        return BlockRegistry.Air.GetDefaultState();
    }


    public void Clear()
    {
        
    }
}