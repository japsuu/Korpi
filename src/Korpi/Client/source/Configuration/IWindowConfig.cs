using Config.Net;

namespace Korpi.Client.Configuration;

/// <summary>
/// Configuration settings for the client window.
/// </summary>
public interface IWindowConfig
{
    /// <summary>
    /// Initial width of the window.
    /// </summary>
    [Option(Alias = "window_width", DefaultValue = 1280)]
    public int WindowWidth { get; set; }
    
    /// <summary>
    /// Initial height of the window.
    /// </summary>
    [Option(Alias = "window_height", DefaultValue = 720)]
    public int WindowHeight { get; set; }

    /// <summary>
    /// Is the window fullscreen?
    /// </summary>
    [Option(Alias = "window_fullscreen", DefaultValue = false)]
    public bool Fullscreen { get; set; }
}