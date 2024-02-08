using Common.Logging;
using Korpi.Networking.Transports;

namespace Korpi.Networking;

public class NetworkManager
{
    private static readonly IKorpiLogger NetworkLogger = LogFactory.GetLogger(typeof(NetworkManager));
    
    public static NetworkManager Instance { get; private set; } = null!;

    private readonly Transport _transport;

    public NetServerManager Server { get; private set; }
    public NetClientManager Client { get; private set; }

    public event Action? Update;


    public static void InitializeSingleton(Transport transportLayer)
    {
        if (Instance != null)
            throw new Exception("NetworkManager already initialized.");
        Instance = new NetworkManager(transportLayer);
    }


    private NetworkManager(Transport transportLayer)
    {
        _transport = transportLayer;
        _transport.Initialize(this);
        Server = new NetServerManager(_transport);
        Client = new NetClientManager(_transport);
    }
    
    
    public void Tick()
    {
        Update?.Invoke();
    }
}