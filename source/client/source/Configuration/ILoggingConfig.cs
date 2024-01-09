using Config.Net;

namespace Korpi.Client.Configuration;

public interface ILoggingConfig
{
    [Option(Alias = "windowWidth", DefaultValue = "false")]
    public bool EnableVerboseLogging { get; }
}