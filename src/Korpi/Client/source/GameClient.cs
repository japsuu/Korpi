using Korpi.Client.Multiplayer;
using Korpi.Server;
using KorpiEngine.Core.Logging;
using KorpiEngine.Networking;
using KorpiEngine.Networking.HighLevel.Connections;
using KorpiEngine.Networking.LowLevel.Transports.EventArgs;
using KorpiEngine.Networking.LowLevel.Transports.LiteNetLib;

namespace Korpi.Client;

/// <summary>
/// Represents a client of a game server.
/// Has capabilities to handle game logic/state and communicate with the game server.
/// </summary>
public sealed class GameClient : IDisposable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GameClient));
    
    private readonly NetworkManager _netManager;
    private GameServer? _localServer;

    /// <summary>
    /// Called when the client has successfully connected to the game server.
    /// </summary>
    public event Action? ConnectedToServer;
    
    /// <summary>
    /// Called when the client has been disconnected from the game server.
    /// </summary>
    public event Action? DisconnectedFromServer;
    
    public GameClient()
    {
        // Initialize the NetworkManager with the transport layer we want to use.
        _netManager = new NetworkManager(new LiteNetLibTransport());
        
        _netManager.Client.Authenticated += OnAuthenticated;
        _netManager.Client.ClientConnectionStateChanged += OnConnectionStateChanged;
    }


    /// <summary>
    /// Connects to the game server at the provided address and port.
    /// </summary>
    public void ConnectToServer(ServerConnectInfo info)
    {
        Logger.Info($"Connecting to {info}...");
        _netManager.Client.Connect(info.Address, info.Port);
    }
    
    
    /// <summary>
    /// Disconnects from the game server.
    /// </summary>
    public void DisconnectFromServer()
    {
        Logger.Info("Disconnecting from server...");
        _netManager.Client.Disconnect();
    }
    
    
    public void StartLocalServer()
    {
        Logger.Info("Creating local server...");
        // Create and start a network game server.
        _localServer = new GameServer(_netManager, GameServerConfiguration.Localhost());
        _localServer.Start();
    }
    
    
    public void StopLocalServer()
    {
        Logger.Info("Stopping local server...");
        _localServer?.Stop();
        _localServer = null;
    }


    private void OnAuthenticated()
    {
        ConnectedToServer?.Invoke();
    }


    private void OnConnectionStateChanged(ClientConnectionStateArgs obj)
    {
        switch (obj.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                DisconnectedFromServer?.Invoke();
                break;
            case LocalConnectionState.Starting:
                break;
            case LocalConnectionState.Started:
                break;
            case LocalConnectionState.Stopping:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    public void Dispose()
    {
        _localServer?.Dispose();
    }
}