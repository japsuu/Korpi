namespace Korpi.Client.Configuration;

public class InMemoryConfig
{
    public readonly bool EnableSelfProfile;
    public readonly string TempDirectory;
    public readonly string SelfProfileOutputFilePath;


    public InMemoryConfig(bool enableSelfProfile)
    {
        EnableSelfProfile = enableSelfProfile;
        TempDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        Directory.CreateDirectory(TempDirectory);
        SelfProfileOutputFilePath = Path.Combine(TempDirectory, "dottrace.tmp");
    }
}