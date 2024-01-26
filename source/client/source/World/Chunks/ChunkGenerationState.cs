namespace Korpi.Client.World.Chunks;

/// <summary>
/// Represents the state of a chunk.
/// </summary>
public enum ChunkGenerationState
{
    /// <summary>
    /// The chunk is uninitialized.
    /// </summary>
    UNINITIALIZED,
    
    /// <summary>
    /// The chunk is queued for terrain generation.
    /// </summary>
    GENERATING_TERRAIN,
    
    /// <summary>
    /// The chunk is queued for decoration generation.
    /// </summary>
    GENERATING_DECORATION,
        
    /// <summary>
    /// The chunk is queued for lighting generation.
    /// </summary>
    GENERATING_LIGHTING,

    /// <summary>
    /// The chunk is ready.
    /// </summary>
    READY
}