using BlockEngine.Framework.Blocks;

namespace BlockEngine.Framework.Bitpacking;

public interface IBlockStorage
{
    public void SetBlock(int index, BlockState block);

    public BlockState GetBlock(int index);
}