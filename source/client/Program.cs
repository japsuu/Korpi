using Korpi.Client.Configuration;
using Korpi.Networking;
using Korpi.Networking.Transports.LiteNetLib;
using Korpi.Server;
using OpenTK.Windowing.Desktop;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

namespace Korpi.Client;

internal static class Program
{
    /// <summary>
    /// Entry point of the application.
    /// Creates and starts the <see cref="GameClientWindow"/>.
    /// </summary>
    /// <param name="args">CLI arguments</param>
    private static void Main(string[] args)
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the NetworkManager with the transport layer we want to use.
        NetworkManager.InitializeSingleton(new LiteNetLibTransport());

        // Create and start a network game server.
        using GameServer server = new(GameServerConfiguration.Default().WithPasswordAuthentication("password", NetworkManager.Instance));
        server.Start();

        // Initialize the client configuration.
        (GameWindowSettings gws, NativeWindowSettings nws) = ClientConfig.Initialize(args);

        // Create and run the game client.
        using GameClientWindow clientWindow = new(gws, nws);
        clientWindow.Run();
        
        // Stop the game server.
        server.Stop();
    }
}