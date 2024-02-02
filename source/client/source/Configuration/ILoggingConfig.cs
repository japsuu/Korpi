using Config.Net;

namespace Korpi.Client.Configuration;

public interface ILoggingConfig
{
    [Option(Alias = "enable_verbose", DefaultValue = "false")]
    public bool EnableVerboseLogging { get; }
}