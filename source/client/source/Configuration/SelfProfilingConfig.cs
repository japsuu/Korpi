namespace Korpi.Client.Configuration;

public class SelfProfilingConfig
{
    /// <summary>
    /// Target file path for the self-profiling agent.
    /// </summary>
    public readonly string SelfProfileTargetPath;


    public SelfProfilingConfig(DirectoryInfo tempDirectory)
    {
        SelfProfileTargetPath = Path.Combine(tempDirectory.FullName, "dottrace.tmp");
    }
}