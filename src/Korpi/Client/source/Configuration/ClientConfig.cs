using Config.Net;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Windowing;
using OpenTK.Mathematics;

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
    /// Render configuration.
    /// </summary>
    public static RenderingConfig Rendering { get; private set; } = null!;
    
    /// <summary>
    /// Profiling configuration.
    /// </summary>
    public static ProfilingConfig Profiling { get; private set; } = null!;


    /// <summary>
    /// Initializes all configuration objects.
    /// </summary>
    /// <param name="args">CLI arguments</param>
    public static WindowingSettings Initialize(string[] args)
    {
        Logger.Info("Initializing configuration...");
        
        // Create necessary directories.
        DirectoryInfo configDirectory = Directory.CreateDirectory("config");
        DirectoryInfo tempDirectory = Directory.CreateDirectory("temp");
        
        // Window configuration
        string windowConfigPath = Path.Combine(configDirectory.FullName, "config_window.json");
        Window = new ConfigurationBuilder<IWindowConfig>()
            .UseJsonFile(windowConfigPath)
            .Build();

        // Render configuration
        Rendering = new RenderingConfig();
        
        // Profiling configuration
        Profiling = new ProfilingConfig(IsSelfProfile(args), tempDirectory);
        
        Logger.Info("Configuration files initialized.");
        return GetWindowSettings();
    }


    private static WindowingSettings GetWindowSettings()
    {
        Vector2i windowSize = new Vector2i(Window.WindowWidth, Window.WindowHeight);
        string windowTitle = $"{Constants.CLIENT_NAME} v{Constants.CLIENT_VERSION}";
        
        return new WindowingSettings(windowSize, windowTitle);
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