using Korpi.Server;
using KorpiEngine.Core.Logging;
using KorpiEngine.Networking;
using KorpiEngine.Networking.HighLevel.Authentication;
using KorpiEngine.Networking.LowLevel.Transports.LiteNetLib;

namespace StandaloneServer;

internal static class Program
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Program));
    
    private static void Main(string[] args)
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the log4net logger configuration.
        LogFactory.Initialize("log4net.config");

        // Try to parse the user-provided arguments.
        if (!TryParseArguments(args, out GameServerConfiguration config))
        {
            Logger.Error("Some required arguments are missing.");
            Logger.Error("Argument usage: <bindAddress:your_address> <bindPort:your_port> [maxConnections:your_max_connections] [password:your_join_password]");
            Console.ReadLine();
            return;
        }

        // Initialize the NetworkManager with the transport layer we want to use.
        NetworkManager netManager = new(new LiteNetLibTransport());

        // Create and start a network game server.
        using GameServer server = new(netManager, config);
        server.Start();
        
        Logger.Info("Type 'stop' to stop the server.");
        bool shouldStop = false;
        while (!shouldStop)
        {
            string? input = Console.ReadLine();
            
            if (input == "stop")
            {
                shouldStop = true;
                server.Stop();
            }
        }
    }


    private static bool TryParseArguments(string[] args, out GameServerConfiguration config)
    {
        config = new GameServerConfiguration();
        
        // Read the arguments.
        foreach (string argument in args)
        {
            string[] split = argument.Split(':');
            if (split.Length != 2)
            {
                Logger.Error($"Invalid argument: {argument}");
                Console.ReadLine();
                return false;
            }

            switch (split[0])
            {
                case "bindAddress":
                    config.BindAddress = split[1];
                    break;
                case "bindPort":
                    if (!ushort.TryParse(split[1], out config.BindPort))
                    {
                        Logger.Error($"Invalid port: {split[1]}");
                        return false;
                    }
                    break;
                case "maxConnections":
                    if (!int.TryParse(split[1], out config.MaxConnections))
                    {
                        Logger.Error($"Invalid max connections: {split[1]}");
                        return false;
                    }
                    break;
                case "password":
                    config.Authenticator = new PasswordAuthenticator(split[1]);
                    break;
                default:
                    Logger.Error($"Invalid argument: {split[0]}");
                    return false;
            }
        }

        // Verify that the required arguments are present.
        return !string.IsNullOrEmpty(config.BindAddress) && config.BindPort != 0;
    }
}