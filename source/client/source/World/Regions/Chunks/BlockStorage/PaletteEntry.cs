using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

public struct PaletteEntry
{
    public int RefCount;
    public BlockState? BlockState;
    
    public bool IsEmpty => BlockState == null || RefCount <= 0;


    public PaletteEntry(int refCount, BlockState? blockState)
    {
        RefCount = refCount;
        BlockState = blockState;
    }
}