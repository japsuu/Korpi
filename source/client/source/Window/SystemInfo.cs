namespace BlockEngine.Client.Window;

/// <summary>
/// Contains information about the current system.
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// Gets the number of processors available to the current process.
    /// </summary>
    public static int ProcessorCount => Environment.ProcessorCount;
}