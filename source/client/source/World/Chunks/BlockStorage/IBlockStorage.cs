using Korpi.Client.Blocks;

namespace Korpi.Client.World.Chunks.BlockStorage;

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
    /// The count of blocks with <see cref="BlockRenderType"/>.<see cref="BlockRenderType.Transparent"/> contained in this <see cref="IBlockStorage"/>.
    /// </summary>
    public int TranslucentBlockCount { get; }

    
    /// <summary>
    /// Sets the block at the given position and returns the old block.
    /// </summary>
    /// <param name="position">Position of the block relative to the chunk.</param>
    /// <param name="block">The block to set.</param>
    /// <param name="oldBlock">The old block that existed in the given location</param>
    public void SetBlock(SubChunkBlockPosition position, BlockState block, out BlockState oldBlock);

    /// <summary>
    /// Gets the block at the given position.
    /// </summary>
    /// <param name="position">Position of the block relative to the chunk.</param>
    /// <returns>The block at the given position. Air if none exists.</returns>
    public BlockState GetBlock(SubChunkBlockPosition position);
    
    /// <summary>
    /// Initializes the block storage with air blocks.
    /// </summary>
    public void Clear();
}