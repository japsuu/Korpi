namespace Korpi.Client.World.Regions.Chunks;

/// <summary>
/// Represents the state of the chunk.
/// </summary>
public enum ChunkState
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
    /// The chunk is waiting for neighbouring chunks to be generated.
    /// </summary>
    WAITING_FOR_MESHING,

    /// <summary>
    /// The chunk is queued for meshing.
    /// </summary>
    MESHING,

    /// <summary>
    /// The chunk is ready.
    /// </summary>
    READY
}