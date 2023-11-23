using Config.Net;

namespace BlockEngine.Framework.Configuration;

public interface IClientSettings
{
    public IWindowSettings WindowSettings { get; }
    public ILoggingSettings LoggingSettings { get; }
}


public interface IWindowSettings
{
    [Option(Alias = "windowWidth", DefaultValue = "1280")]
    public int WindowWidth { get; }
    [Option(Alias = "windowHeight", DefaultValue = "720")]
    public int WindowHeight { get; }
}



public interface ILoggingSettings
{
    [Option(Alias = "windowWidth", DefaultValue = "false")]
    public bool EnableVerboseLogging { get; }
}
