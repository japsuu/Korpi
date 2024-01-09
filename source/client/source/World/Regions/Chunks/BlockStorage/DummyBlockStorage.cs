using Korpi.Client.Registries;
using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

public class DummyBlockStorage : IBlockStorage
{
    public int RenderedBlockCount => 0;
    
    
    public void SetBlock(int x, int y, int z, BlockState block, out BlockState oldBlock)
    {
        oldBlock = BlockRegistry.Air.GetDefaultState();
    }


    public BlockState GetBlock(int x, int y, int z)
    {
        return BlockRegistry.Air.GetDefaultState();
    }
}