using Config.Net;
using Korpi.Client.Logging;

namespace Korpi.Client.Configuration;

/// <summary>
/// Holds all configuration objects.
/// </summary>
public static class ClientConfig
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ClientConfig));
    
    /// <summary>
    /// Window configuration.
    /// </summary>
    public static IWindowConfig WindowConfig { get; private set; } = null!;
    
    /// <summary>
    /// Logging configuration.
    /// </summary>
    public static ILoggingConfig LoggingConfig { get; private set; } = null!;

#if DEBUG
    /// <summary>
    /// Debug mode configuration. Only available in debug builds.
    /// </summary>
    public static DebugModeConfig DebugModeConfig { get; private set; } = null!;
#endif


    /// <summary>
    /// Initializes all configuration objects.
    /// </summary>
    /// <param name="args">CLI arguments</param>
    public static void Initialize(IReadOnlyList<string> args)
    {
        Logger.Info("Initializing configuration...");
        WindowConfig = new ConfigurationBuilder<IWindowConfig>()
            .UseJsonFile("WindowConfig.json")
            .Build();
        
        LoggingConfig = new ConfigurationBuilder<ILoggingConfig>()
            .UseJsonFile("LoggingConfig.json")
            .Build();

#if DEBUG
        
        // Check if args contains the "-photomode" flag
        bool isPhotoMode = false;
        string photoModePath = "Screenshots";
        if (args.Count > 0 && args[0] == "-photomode")
        {
            isPhotoMode = true;
            Logger.Warn("Running in photo mode...");
            if (args.Count > 1)
                photoModePath = args[1];
            else
                Logger.Warn($"No path specified for photo mode, using default path \"{photoModePath}\".");
        }
        
        DebugModeConfig = new DebugModeConfig(isPhotoMode, photoModePath);
#endif
        
        Logger.Info("Configuration initialized.");
    }
}