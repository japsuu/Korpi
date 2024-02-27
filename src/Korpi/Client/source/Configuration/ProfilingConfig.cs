namespace Korpi.Client.Configuration;

public class ProfilingConfig
{
    /// <summary>
    /// If the self-profiling agent should be enabled.
    /// </summary>
    public readonly bool EnableSelfProfile;
    
    /// <summary>
    /// Target file path for the self-profiling agent.
    /// </summary>
    public readonly string SelfProfileTargetPath;


    public ProfilingConfig(bool enableSelfProfile, DirectoryInfo tempDirectory)
    {
        EnableSelfProfile = enableSelfProfile;
        SelfProfileTargetPath = Path.Combine(tempDirectory.FullName, "dottrace.tmp");
    }
}