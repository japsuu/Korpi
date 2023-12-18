using Config.Net;

namespace BlockEngine.Client.Framework.Configuration;

public interface ILoggingConfig
{
    [Option(Alias = "windowWidth", DefaultValue = "false")]
    public bool EnableVerboseLogging { get; }
}