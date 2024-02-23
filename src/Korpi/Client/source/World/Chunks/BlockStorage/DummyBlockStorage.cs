using Korpi.Client.Blocks;
using Korpi.Client.Registries;

namespace Korpi.Client.World.Chunks.BlockStorage;

public class DummyBlockStorage : IBlockStorage
{
    public int RenderedBlockCount => 0;
    public int TranslucentBlockCount => 0;
    
    
    public void SetBlock(ChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        oldBlock = BlockRegistry.Air.GetDefaultState();
    }


    public BlockState GetBlock(ChunkBlockPosition position)
    {
        return BlockRegistry.Air.GetDefaultState();
    }


    public void Clear()
    {
        
    }
}