using Korpi.Client.Configuration;
using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
namespace Korpi.Client;

internal static class Program
{
    /// <summary>
    /// Entry point of the application.
    /// Creates and starts the <see cref="GameClient"/>.
    /// </summary>
    /// <param name="args">CLI arguments</param>
    private static void Main(string[] args)
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the configuration.
        (GameWindowSettings gws, NativeWindowSettings nws) = ClientConfig.Initialize(args);
        
        // Create and run the game client.
        using GameClient client = new(gws, nws);
        client.Run();
    }
}