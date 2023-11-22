using Config.Net;

namespace BlockEngine.Framework.Configuration;

public static class Settings
{
    public static IClientSettings Client { get; private set; } = null!;


    public static void Initialize()
    {
        Client = new ConfigurationBuilder<IClientSettings>()
            .UseAppConfig()
            .Build();
    }
}