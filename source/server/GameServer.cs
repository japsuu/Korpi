using Korpi.Networking;

namespace Korpi.Server;

/// <summary>
/// Represents a game server.
/// Has capabilities to handle game logic/state and communicate with clients.
/// </summary>
public class GameServer : IDisposable
{
    protected readonly GameServerConfiguration Configuration;
    
    public event Action? ServerStarted;
    public event Action? ServerStopped;


    /// <param name="configuration">The configuration to use</param>
    public GameServer(GameServerConfiguration configuration)
    {
        Configuration = configuration;
        NetworkManager.Server.SetMaxConnections(Configuration.MaxConnections);
    }


    /// <summary>
    /// Starts the game server using the provided configuration.
    /// </summary>
    public void Start()
    {
        NetworkManager.Server.StartServer(Configuration.BindAddress, Configuration.BindPort);
        OnStart();
        ServerStarted?.Invoke();
    }


    /// <summary>
    /// Stops the game server.
    /// </summary>
    public void Stop()
    {
        NetworkManager.Server.StopServer();
        OnStop();
        ServerStopped?.Invoke();
    }


    /// <summary>
    /// Invoked when the server is started.
    /// </summary>
    protected virtual void OnStart() { }
    
    /// <summary>
    /// Invoked when the server is stopped.
    /// </summary>
    protected virtual void OnStop() { }


    public virtual void Dispose()
    {
        // TODO release managed resources here
        GC.SuppressFinalize(this);
    }
}