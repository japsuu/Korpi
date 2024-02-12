using Korpi.Networking.HighLevel.Messages;
using Korpi.Networking.LowLevel.NetStack.Buffers;
using Korpi.Networking.LowLevel.NetStack.Serialization;
using Korpi.Networking.Transports;

namespace Korpi.Networking;

public class TransportManager
{
    private readonly NetworkManager _netManager;
    private readonly ArrayPool<byte> _byteBufferPool = ArrayPool<byte>.Create(1024, 50);
    private readonly BitBuffer _bitBuffer = new();
    
    public readonly Transport Transport;

    public event Action<bool>? IterateOutgoingStart;
    public event Action<bool>? IterateOutgoingEnd;
    public event Action<bool>? IterateIncomingStart;
    public event Action<bool>? IterateIncomingEnd;


    public TransportManager(NetworkManager netManager, Transport transport)
    {
        _netManager = netManager;
        Transport = transport;
        MessageManager.RegisterAllMessages();
    }
    
    
    public string GetConnectionAddress(int clientId) => Transport.GetRemoteConnectionAddress(clientId);
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
        Transport.StartLocalConnection(isServer);
    }


    public void StopConnection(bool isServer)
    {
        Transport.StopLocalConnection(isServer);
    }


    public void StopConnection(int clientId, bool immediate)
    {
        Transport.StopRemoteConnection(clientId, immediate);
    }


    public void SendToClient<T>(Channel channel, T packet, int clientId) where T : NetMessage
    {
        packet.Serialize(_bitBuffer);
        byte[] byteBuffer = _byteBufferPool.Rent(_bitBuffer.Length);
        int length = _bitBuffer.ToArray(byteBuffer);
        ArraySegment<byte> segment = new(byteBuffer, 0, length);
        Transport.SendToClient(channel, segment, clientId);
        _byteBufferPool.Return(byteBuffer);
    }


    public void SendToServer<T>(Channel channel, T packet) where T : NetMessage
    {
        packet.Serialize(_bitBuffer);
        byte[] byteBuffer = _byteBufferPool.Rent(_bitBuffer.Length);
        int length = _bitBuffer.ToArray(byteBuffer);
        ArraySegment<byte> segment = new(byteBuffer, 0, length);
        Transport.SendToServer(channel, segment);
        _byteBufferPool.Return(byteBuffer);
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