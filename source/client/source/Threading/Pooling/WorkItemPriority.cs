namespace BlockEngine.Client.Threading.Pooling;

/// <summary>
/// The priority of a work item.
/// </summary>
public enum WorkItemPriority
{
    /// <summary>
    /// The lowest priority.
    /// </summary>
    Low,
    /// <summary>
    /// The default priority.
    /// </summary>
    Normal,
    /// <summary>
    /// A high priority.
    /// </summary>
    High,
    /// <summary>
    /// The task should be executed as soon as possible.
    /// </summary>
    Critical
}