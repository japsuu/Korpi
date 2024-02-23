namespace Korpi.Client.World.Chunks;

/// <summary>
/// Represents the state of a chunk mesh.
/// </summary>
public enum ChunkMeshState
{
    /// <summary>
    /// The mesh is uninitialized.
    /// </summary>
    UNINITIALIZED,
    
    /// <summary>
    /// The chunk is waiting for neighbouring chunks to be generated.
    /// </summary>
    WAITING_FOR_NEIGHBOURS,

    /// <summary>
    /// The chunk is queued for meshing.
    /// </summary>
    MESHING,

    /// <summary>
    /// The chunk is ready.
    /// </summary>
    READY
}