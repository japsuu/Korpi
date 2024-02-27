using Korpi.Client.World.Chunks;

namespace Korpi.Client.Generation.TerrainGenerators;

/// <summary>
/// Represents an object that can populate a <see cref="Chunk"/> with voxel data.
/// </summary>
public interface ITerrainGenerator
{
    /// <summary>
    /// Populates a given chunkColumn with voxel data.
    /// The chunkColumn is expected to be empty before calling this.
    /// This function is expected to be thread-safe as generation is executed on the pool.
    /// </summary>
    public void ProcessChunk(in ChunkColumn chunkColumn);
}