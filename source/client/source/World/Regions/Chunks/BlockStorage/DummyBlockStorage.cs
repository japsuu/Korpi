using Korpi.Client.Registries;
using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

public class DummyBlockStorage : IBlockStorage
{
    public int RenderedBlockCount => 0;
    
    
    public void SetBlock(ChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        oldBlock = BlockRegistry.Air.GetDefaultState();
    }


    public BlockState GetBlock(ChunkBlockPosition position)
    {
        return BlockRegistry.Air.GetDefaultState();
    }
}