using Common.Logging;
using Korpi.Client.Configuration;
using Korpi.Networking;
using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using OpenTK.Windowing.Desktop;

namespace Korpi.Client;

/// <summary>
/// Represents a client of a game server.
/// Has capabilities to handle game logic/state and communicate with the game server.
/// </summary>
public class GameClient : IDisposable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GameClient));
    
    private readonly NetworkManager _netManager;
    private readonly string[] _args;
    private GameClientWindow _window;
    
    public GameClient(NetworkManager netManager, string[] args)
    {
        _netManager = netManager;
        _args = args;
    }


    /// <summary>
    /// Enters a blocking loop to run the game client.
    /// </summary>
    public void Run()
    {
        Logger.Info("Starting client...");
        _netManager.Client.Connect("", 0);
        
        // Block until _isConnected is true.
        while (!_netManager.Client.Started)
        {
            Thread.Sleep(100);
        }
        Thread.Sleep(3000);
        Logger.Info("Stopping client...");
        _netManager.Client.Disconnect();
        return;
        
        // Initialize the client configuration.
        (GameWindowSettings gws, NativeWindowSettings nws) = ClientConfig.Initialize(_args);
        
        // Create and run the game client window.
        _window = new GameClientWindow(gws, nws);

        _window.Run();
    }


    public void Dispose()
    {
        _window?.Dispose();
    }
}