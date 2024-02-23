using System.Reflection;
using Korpi.Client.Configuration;
using Korpi.Networking;
using Korpi.Networking.LowLevel.Transports.LiteNetLib;
using Korpi.Server;
using log4net;
using log4net.Config;
using log4net.Repository;
using OpenTK.Windowing.Desktop;

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
        InitializeLog4Net();
        
        // Initialize the client configuration.
        (GameWindowSettings gws, NativeWindowSettings nws) = ClientConfig.Initialize(args);
        
        // Create and run the client window.
        using ClientWindow window = new(gws, nws);
        window.Run();
    }


    private static void InitializeLog4Net()
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the Log4Net configuration.
        ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
    }
}