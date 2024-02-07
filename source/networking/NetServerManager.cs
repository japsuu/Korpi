namespace Korpi.Networking;

/// <summary>
/// Manages the network server.
/// Does not deal with game logic, only with network communication.
/// </summary>
public class NetServerManager
{
    private NetworkTransport Transport { get; }
    
    
    public NetServerManager(NetworkTransport transport)
    {
        Transport = transport;
    }


    public void SetMaxConnections(int maxConnections)
    {
        Transport.MaxConnections = maxConnections;
    }


    public void StartServer(string address, ushort port)
    {
        Transport.ServerBindAddress = address;
        Transport.ServerBindPort = port;
        Transport.StartServer();
    }
    
    
    public void StopServer()
    {
        Transport.StopServer();
    }
}