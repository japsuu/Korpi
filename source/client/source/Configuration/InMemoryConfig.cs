namespace Korpi.Client.Configuration;

public class InMemoryConfig
{
    public readonly bool EnableSelfProfile;
    public readonly string SelfProfileOutputDirectory;
    public readonly string SelfProfileOutputFilePath;


    public InMemoryConfig(bool enableSelfProfile)
    {
        EnableSelfProfile = enableSelfProfile;
        SelfProfileOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        Directory.CreateDirectory(SelfProfileOutputDirectory);
        SelfProfileOutputFilePath = Path.Combine(SelfProfileOutputDirectory, "dottrace.tmp");
    }
}