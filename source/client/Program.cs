using Korpi.Client.Configuration;
using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
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
        
        ClientConfig.Initialize(args);

        GameWindowSettings gws = new()
        {
            UpdateFrequency = Constants.UPDATE_FRAME_FREQUENCY
        };
        
        NativeWindowSettings nws = new()
        {
            Size = new Vector2i(ClientConfig.WindowConfig.WindowWidth, ClientConfig.WindowConfig.WindowHeight),
            Title = $"{Constants.CLIENT_NAME} v{Constants.CLIENT_VERSION}",
            NumberOfSamples = 0,
            Location = new Vector2i(200, 0),
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 2),
            Icon = IoUtils.GetIcon(),
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        };
        using GameClient client = new(gws, nws);
            
        client.Run();
    }
}