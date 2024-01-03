using Config.Net;

namespace BlockEngine.Client.Configuration;

public interface ILoggingConfig
{
    [Option(Alias = "windowWidth", DefaultValue = "false")]
    public bool EnableVerboseLogging { get; }
}