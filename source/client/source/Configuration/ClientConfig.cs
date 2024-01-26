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
    
    public static InMemoryConfig Store { get; private set; } = null!;


    /// <summary>
    /// Initializes all configuration objects.
    /// </summary>
    /// <param name="args">CLI arguments</param>
    public static void Initialize(string[] args)
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
        if (args.ContainsFlag("-photomode", out string? suppliedPath))
        {
            isPhotoMode = true;
            Logger.Warn("Running in photo mode...");
            if (suppliedPath != null)
            {
                photoModePath = suppliedPath;
                Logger.Warn($"Using path \"{photoModePath}\" for photo mode.");
            }
            else
                Logger.Warn($"No path specified for photo mode, using default path \"{photoModePath}\".");
        }
        
        DebugModeConfig = new DebugModeConfig(isPhotoMode, photoModePath);
#endif
        
        bool enableSelfProfiling = args.Contains("-selfprofile");
        if (enableSelfProfiling)
            Logger.Warn("Self-profiling enabled.");
        Store = new InMemoryConfig(enableSelfProfiling);
        
        Logger.Info("Configuration initialized.");
    }
    
    
    /// <summary>
    /// Checks if the given flag is present in the given args array.
    /// </summary>
    /// <param name="args">The args array to check.</param>
    /// <param name="flag">The flag to check for.</param>
    /// <param name="value">The value of the flag, if present. Null otherwise.</param>
    /// <returns>True if the flag is present, false otherwise.</returns>
    private static bool ContainsFlag(this string[] args, string flag, out string? value)
    {
        if (args.Contains(flag))
        {
            int index = Array.IndexOf(args, flag);
            if (index + 1 < args.Length)
            {
                value = args[index + 1];
                return true;
            }
            value = null;
            return true;
        }

        value = null;
        return false;
    }
}