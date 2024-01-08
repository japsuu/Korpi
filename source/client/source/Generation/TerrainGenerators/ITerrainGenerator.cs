using BlockEngine.Client.World.Regions.Chunks;

namespace BlockEngine.Client.Generation;

/// <summary>
/// Represents an object that can populate a <see cref="Chunk"/> with voxel data.
/// </summary>
public interface ITerrainGenerator
{
    /// <summary>
    /// Populates a given chunk with voxel data.
    /// The chunk is expected to be empty before calling this.
    /// This function is expected to be thread-safe as generation is executed on the pool.
    /// </summary>
    public void ProcessChunk(in Chunk chunk);
}