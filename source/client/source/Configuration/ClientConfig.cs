using Config.Net;
using Korpi.Client.Logging;
using Korpi.Client.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

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
    public static IWindowConfig Window { get; private set; } = null!;
    
    /// <summary>
    /// Logging configuration.
    /// </summary>
    public static ILoggingConfig Logging { get; private set; } = null!;

#if DEBUG
    /// <summary>
    /// Debug mode configuration. Only available in debug builds.
    /// </summary>
    public static DebugModeConfig Debugging { get; private set; } = null!;
#endif
    
    /// <summary>
    /// Self profiling configuration.
    /// Null if self-profiling is not enabled.
    /// </summary>
    public static SelfProfilingConfig? Profiling { get; private set; }


    /// <summary>
    /// Initializes all configuration objects.
    /// </summary>
    /// <param name="args">CLI arguments</param>
    public static (GameWindowSettings gws, NativeWindowSettings nws) Initialize(string[] args)
    {
        Logger.Info("Initializing configuration...");
        
        // Create necessary directories.
        DirectoryInfo configDirectory = Directory.CreateDirectory("config");
        DirectoryInfo tempDirectory = Directory.CreateDirectory("temp");
        
        // Window configuration
        string windowConfigPath = Path.Combine(configDirectory.FullName, "config_window.json");
        File.Create(windowConfigPath).Close();
        Window = new ConfigurationBuilder<IWindowConfig>()
            .UseJsonFile(windowConfigPath)
            .Build();
        
        // Logging configuration
        string loggingConfigPath = Path.Combine(configDirectory.FullName, "config_logging.json");
        File.Create(loggingConfigPath).Close();
        Logging = new ConfigurationBuilder<ILoggingConfig>()
            .UseJsonFile(loggingConfigPath)
            .Build();

#if DEBUG
        // Debug configuration
        Debugging = new DebugModeConfig();
#endif
        
        // Self profiling configuration
        if (IsSelfProfile(args))
            Profiling = new SelfProfilingConfig(tempDirectory);
        
        Logger.Info("Configuration files initialized.");
        return GetWindowSettings();
    }


    private static (GameWindowSettings gws, NativeWindowSettings nws) GetWindowSettings()
    {
        GameWindowSettings gws = new()
        {
            UpdateFrequency = Constants.UPDATE_FRAME_FREQUENCY
        };
        
        NativeWindowSettings nws = new()
        {
            Size = new Vector2i(Window.WindowWidth, Window.WindowHeight),
            StartVisible = false,
            Title = $"{Constants.CLIENT_NAME} v{Constants.CLIENT_VERSION}",
            Icon = IoUtils.GetIcon(),
            NumberOfSamples = 0,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 2),
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        };
        
        return (gws, nws);
    }


    private static bool IsSelfProfile(string[] args)
    {
        bool enableSelfProfiling = args.Contains("-selfprofile");
        if (enableSelfProfiling)
            Logger.Warn("Self-profiling enabled.");
        return enableSelfProfiling;
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