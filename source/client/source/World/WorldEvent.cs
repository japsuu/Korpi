namespace Korpi.Client.World;

public enum WorldEvent
{
    /// <summary>
    /// Fired when the load region moves to a new chunk.
    /// </summary>
    LOAD_REGION_CHANGED,
    
    /// <summary>
    /// Fired when all chunks need to be reloaded.
    /// </summary>
    RELOAD_ALL_CHUNKS,
}