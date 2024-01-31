using Korpi.Client.Blocks;

namespace Korpi.Client.Generation;

public interface IWorldInfo
{
    public string Name { get; }
    public int Seed { get; }
    public int WorldHeight { get; }
}

public abstract class BlockPopulator
{
    /// <summary>
    /// Populates an area of blocks at or around the given chunk.
    ///
    /// This method should never attempt to get the Chunk at the passed coordinates, as doing so may cause an infinite loop.
    /// This method should never modify a LimitedRegion at a later point of time.
    /// This method must be completely thread safe and able to handle multiple concurrent callers.
    /// </summary>
    public abstract void Populate(IWorldInfo world, Random rng, int chunkX, int chunkZ, ILimitedRegion region);
}

/// <summary>
/// Represents a limited region of blocks, that is used in world generation for features that can go over a chunk.
/// For example trees and structures.
/// </summary>
public interface ILimitedRegion : IRegionAccessor
{
    /// <summary>
    /// Gets the buffer size around the central chunk which is accessible.
    /// For example: If this returns 16 you have a working area of (16+32+16)*(16+32+16).
    /// </summary>
    public int BufferSize { get; }
    
    /// <summary>
    /// Checks if the given coordinates are inside the region.
    /// </summary>
    public bool IsInRegion(int x, int y, int z);
}

/// <summary>
/// Represents access to a region, gives access to get/set blocks inside the region.
/// </summary>
public interface IRegionAccessor
{
    public BlockState GetBlockState(int x, int y, int z);
    public BlockState SetBlockState(int x, int y, int z, BlockState newBlockState);
}