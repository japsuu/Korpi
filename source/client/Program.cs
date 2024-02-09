using System.Reflection;
using Korpi.Networking;
using Korpi.Networking.Transports.Singleplayer;
using Korpi.Server;
using log4net;
using log4net.Config;
using log4net.Repository;

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
        
        // Initialize the log4net logger configuration.
        ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        
        // Initialize the NetworkManager with the transport layer we want to use.
        NetworkManager netManager = new(new SingleplayerTransport());

        // Create and start a network game server.
        using GameServer server = new(netManager, GameServerConfiguration.Default().WithPasswordAuthentication("password"));
        server.Start();
        
        while (!netManager.Server.Started)
        {
            Thread.Sleep(100);
        }

        // Create and run the game client.
        using GameClient client = new(netManager, args);
        client.Run();
        
        // Stop the game server.
        server.Stop();
    }
}