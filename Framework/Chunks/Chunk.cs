using BlockEngine.Framework.Bitpacking;
using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Chunks;

public class Chunk
{
    private readonly BlockPalette _blocks;
        
        
    public Chunk(BlockPalette data)
    {
        _blocks = data;
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public void SetBlockState(Vector3i position, BlockState block)
    {
        // Use (z << (W + H)) + (y << W) + x to index into a 3D array stored as a 1D array.
        // So, in this formula, "W" and "H" are the logarithms (base 2) of the dimensions of the array in the x and y directions, respectively.
        // In other words, if the width and height of the 3D array are both 2^N, then "W" and "H" would be equal to "N."
        int index = (position.Z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (position.Y << Constants.CHUNK_SIZE_LOG2) + position.X;
        _blocks.SetBlock(index, block);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(Vector3i position)
    {
        // Use (z << (W + H)) + (y << W) + x to index into a 3D array stored as a 1D array.
        // So, in this formula, "W" and "H" are the logarithms (base 2) of the dimensions of the array in the x and y directions, respectively.
        // In other words, if the width and height of the 3D array are both 2^N, then "W" and "H" would be equal to "N."
        int index = (position.Z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (position.Y << Constants.CHUNK_SIZE_LOG2) + position.X;
        return _blocks.GetBlock(index);
    }


    public void Tick(double deltaTime)
    {
            
    }
}