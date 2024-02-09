using Common.Logging;
using Korpi.Networking;

namespace Korpi.Server;

/// <summary>
/// Represents a game server.
/// Has capabilities to handle game logic/state and communicate with clients.
/// </summary>
public class GameServer : IDisposable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GameServer));

    private readonly NetworkManager _netManager;
    protected readonly GameServerConfiguration Configuration;
    private volatile bool _shouldStop;
    
    public event Action? ServerStarted;
    public event Action? ServerStopped;


    /// <param name="netManager">The network manager to use</param>
    /// <param name="configuration">The configuration to use</param>
    public GameServer(NetworkManager netManager, GameServerConfiguration configuration)
    {
        _netManager = netManager;
        Configuration = configuration;
        _netManager.Server.SetMaxConnections(Configuration.MaxConnections);
        _netManager.Server.SetAuthenticator(Configuration.Authenticator);
    }


    /// <summary>
    /// Starts the game server using the provided configuration.
    /// </summary>
    public void Start()
    {
        Logger.Info("Starting server...");
        _netManager.Server.StartServer(Configuration.BindAddress, Configuration.BindPort);
        OnStart();
        Task.Run(WorkLoop);
        ServerStarted?.Invoke();
    }
    
    
    public void WorkLoop()
    {
        //TODO: Implement a proper game loop here.
        while (!_shouldStop)
        {
            _netManager.Tick();
        }
    }


    /// <summary>
    /// Stops the game server.
    /// </summary>
    public void Stop()
    {
        Logger.Info("Stopping server...");
        _shouldStop = true;
        _netManager.Server.StopServer(true);
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