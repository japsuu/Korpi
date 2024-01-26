namespace Korpi.Client.Configuration;

public class InMemoryConfig
{
    public readonly bool EnableSelfProfile;


    public InMemoryConfig(bool enableSelfProfile)
    {
        EnableSelfProfile = enableSelfProfile;
    }
}