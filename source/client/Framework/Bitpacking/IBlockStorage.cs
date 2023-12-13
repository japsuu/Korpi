using BlockEngine.Client.Framework.Blocks;

namespace BlockEngine.Client.Framework.Bitpacking;

public interface IBlockStorage
{
    public void SetBlock(int x, int y, int z, BlockState block);

    public BlockState GetBlock(int x, int y, int z);
}