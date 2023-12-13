using BlockEngine.Client.Framework.Blocks;

namespace BlockEngine.Client.Framework.Bitpacking;

public struct PaletteEntry
{
    public int RefCount;
    public BlockState? BlockState;     // WARN: Changing the state / data of a block can have unintended consequences! Find out if it's better to store just the block's ID/type instead.


    public PaletteEntry(int refCount, BlockState? blockState)
    {
        RefCount = refCount;
        BlockState = blockState;
    }
}