using Korpi.Client.World.Chunks;

namespace Korpi.Client.Generation.TerrainGenerators;

/// <summary>
/// Represents an object that can populate a <see cref="SubChunk"/> with voxel data.
/// </summary>
public interface ITerrainGenerator
{
    /// <summary>
    /// Lightweight method to check if this chunk would be processed by this generator.
    /// For example if the chunk is full of air, it would be skipped by the terrain generator.
    /// </summary>
    /// <param name="chunk">The chunk to check.</param>
    /// <returns>True if the chunk would be processed by this generator, false otherwise.</returns>
    public bool WillProcessChunk(Chunk chunk);
    
    
    /// <summary>
    /// Populates a given chunk with voxel data.
    /// The chunk is expected to be empty before calling this.
    /// This function is expected to be thread-safe as generation is executed on the pool.
    /// </summary>
    public void ProcessChunk(in Chunk chunk);
}