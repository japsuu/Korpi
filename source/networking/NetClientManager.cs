namespace Korpi.Networking;

/// <summary>
/// Manages the network client.
/// Does not deal with game logic, only with network communication.
/// </summary>
public class NetClientManager
{
    private NetworkTransport Transport { get; }
    
    
    public NetClientManager(NetworkTransport transport)
    {
        Transport = transport;
    }


    public void Connect(string address, ushort port)
    {
        Transport.ClientConnectAddress = address;
        Transport.ClientConnectPort = port;
        Transport.StartClient();
    }
    
    
    public void Disconnect()
    {
        Transport.StopClient();
    }
}