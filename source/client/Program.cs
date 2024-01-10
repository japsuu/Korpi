using Korpi.Client.Configuration;
using Korpi.Client.Window;

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
        ClientConfig.Initialize(args);

        using GameClient client = new();
            
        client.Run();
    }
}