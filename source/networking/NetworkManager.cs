using Common.Logging;
using Korpi.Networking.Connections;
using Korpi.Networking.Transports;

namespace Korpi.Networking;

public class NetworkManager
{
    private static readonly IKorpiLogger NetworkLogger = LogFactory.GetLogger(typeof(NetworkManager));
    
    public static NetworkManager Instance { get; private set; } = null!;

    public readonly TransportManager TransportManager;
    public readonly NetServerManager Server;
    public readonly NetClientManager Client;

    public event Action? Update;


    public static void InitializeSingleton(Transport transportLayer)
    {
        if (Instance != null)
            throw new Exception("NetworkManager already initialized.");
        Instance = new NetworkManager(transportLayer);
    }


    private NetworkManager(Transport transportLayer)
    {
        transportLayer.Initialize(this);
        TransportManager = new TransportManager(this, transportLayer);
        Server = new NetServerManager(this, TransportManager);
        Client = new NetClientManager(this, TransportManager);
    }
    
    
    public void Tick()
    {
        Update?.Invoke();
    }


    public static void ClearClientsCollection(Dictionary<int,NetworkConnection> clients)
    {
        // Dispose of all clients and clear the collection.
        foreach (NetworkConnection client in clients.Values)
        {
            client.Dispose();
        }
        clients.Clear();
    }
}