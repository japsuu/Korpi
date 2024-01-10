using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

/// <summary>
/// Represents a storage for block state data.
/// </summary>
public interface IBlockStorage
{
    /// <summary>
    /// The count of blocks with <see cref="BlockRenderType"/> other than <see cref="BlockRenderType.None"/> (other than air) contained in this <see cref="IBlockStorage"/>.
    /// </summary>
    public int RenderedBlockCount { get; }


    /// <summary>
    /// Sets the block at the given position.
    /// </summary>
    /// <param name="position">Position of the block relative to the chunk.</param>
    /// <param name="block">The block to set.</param>
    /// <param name="oldBlock">The old block that existed in the given location</param>
    public void SetBlock(ChunkBlockPosition position, BlockState block, out BlockState oldBlock);

    /// <summary>
    /// Gets the block at the given position.
    /// </summary>
    /// <param name="position">Position of the block relative to the chunk.</param>
    /// <returns>The block at the given position. Air if none exists.</returns>
    public BlockState GetBlock(ChunkBlockPosition position);
}