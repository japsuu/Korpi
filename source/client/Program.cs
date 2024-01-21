using Korpi.Client.Configuration;
using Korpi.Client.Window;

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

        using GameClient client = new();
            
        client.Run();
    }
}