using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;
using Korpi.Networking.Transports;

namespace Korpi.Networking;

public class TransportManager
{
    private readonly NetworkManager _netManager;
    public readonly Transport Transport;

    public event Action<ClientReceivedDataArgs>? ClientReceivedPacket;
    public event Action<ServerReceivedPacketArgs>? ServerReceivedPacket;
    public event Action<ClientConnectionStateArgs>? ClientConnectionStateChanged;
    public event Action<RemoteConnectionStateArgs>? RemoteConnectionStateChanged;
    public event Action<ServerConnectionStateArgs>? ServerConnectionStateChanged;
    public event Action<bool>? IterateOutgoingStart;
    public event Action<bool>? IterateOutgoingEnd;
    public event Action<bool>? IterateIncomingStart;
    public event Action<bool>? IterateIncomingEnd;


    public TransportManager(NetworkManager netManager, Transport transport)
    {
        _netManager = netManager;
        Transport = transport;
        SubscribeEvents();
    }


    private void SubscribeEvents()
    {
        Transport.ClientReceivedPacket += ClientReceivedPacket;
        Transport.ServerReceivedPacket += ServerReceivedPacket;
        Transport.ClientConnectionStateChanged += ClientConnectionStateChanged;
        Transport.RemoteConnectionStateChanged += RemoteConnectionStateChanged;
        Transport.ServerConnectionStateChanged += ServerConnectionStateChanged;
    }
    
    
    public string GetConnectionAddress(int clientId) => Transport.GetConnectionAddress(clientId);
    public string GetClientAddress() => Transport.GetClientAddress();
    public ushort GetPort() => Transport.GetPort();


    public void SetClientAddress(string address)
    {
        Transport.SetClientAddress(address);
    }


    public void SetServerBindAddress(string address)
    {
        Transport.SetServerBindAddress(address);
    }


    public void SetPort(ushort port)
    {
        Transport.SetPort(port);
    }


    public void SetMaximumClients(int maxConnections)
    {
        Transport.SetMaximumClients(maxConnections);
    }


    public void StartConnection(bool isServer)
    {
        Transport.StartConnection(isServer);
    }


    public void StopConnection(bool isServer)
    {
        Transport.StopConnection(isServer);
    }


    public void StopConnection(int clientId, bool immediate)
    {
        Transport.StopConnection(clientId, immediate);
    }


    public void SendToClient<T>(Channel channel, T packet, int clientId) where T : struct, IPacket
    {
        Transport.SendToClient(channel, packet, clientId);
    }


    public void SendToServer<T>(Channel channel, T packet) where T : struct, IPacket
    {
        Transport.SendToServer(channel, packet);
    }


    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="asServer">True to process data received on the server.</param>
    internal void IterateIncoming(bool asServer)
    {
        IterateIncomingStart?.Invoke(asServer);
        Transport.IterateIncoming(asServer);
        IterateIncomingEnd?.Invoke(asServer);
    }


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="asServer">True to process data to be sent on the server.</param>
    public void IterateOutgoing(bool asServer)
    {
        IterateOutgoingStart?.Invoke(asServer);
        Transport.IterateOutgoing(asServer);
        IterateOutgoingEnd?.Invoke(asServer);
    }
}