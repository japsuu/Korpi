using Common.Logging;
using Korpi.Networking.Authenticating;
using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;
using Korpi.Networking.Packets.Handlers;
using Korpi.Networking.Transports;
using Korpi.Networking.Utility;

namespace Korpi.Networking;

/// <summary>
/// Manages the network client.
/// Does not deal with game logic, only with network communication.
/// </summary>
public class NetClientManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetClientManager));
    private readonly Dictionary<ushort, PacketHandlerCollection> _packetHandlers = new();
    private Transport Transport { get; }
    
    /// <summary>
    /// NetworkConnection of the local client.
    /// </summary>
    public NetworkConnection? Connection;

    /// <summary>
    /// All currently connected clients (peers) by clientId.
    /// </summary>
    public readonly Dictionary<int, NetworkConnection> Clients = new();

    /// <summary>
    /// Called after local client has authenticated.
    /// </summary>
    public event Action? OnAuthenticated;
    
    /// <summary>
    /// Called after the local client connection state changes.
    /// </summary>
    public event Action<ClientConnectionStateArgs>? OnClientConnectionState;
    
    /// <summary>
    /// Called when a client other than self connects.
    /// This is only available when using ServerManager.ShareIds.
    /// </summary>
    public event Action<RemoteConnectionStateArgs>? OnRemoteConnectionState;
    
    
    public NetClientManager(Transport transport)
    {
        Transport = transport;
    }


    public void Connect(string address, ushort port)
    {
        Transport.SetClientAddress(address);
        Transport.SetPort(port);
        Transport.StartConnection(false);
    }
    
    
    public void Disconnect()
    {
        Transport.StopConnection(false);
    }


    /// <summary>
    /// Registers a method to call when a packet of the specified type arrives.
    /// </summary>
    /// <param name="handler">Method to call.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to send this packet.</param>
    /// <typeparam name="T"></typeparam>
    public void RegisterPacketHandler<T>(Action<NetworkConnection, T, Channel> handler, bool requireAuthenticated = true) where T : struct, IPacket
    {
        ushort key = PacketHelper.GetKey<T>();

        if (!_packetHandlers.TryGetValue(key, out PacketHandlerCollection? packetHandler))
        {
            packetHandler = new ClientPacketHandler<T>(requireAuthenticated);
            _packetHandlers.Add(key, packetHandler);
        }

        packetHandler.RegisterHandler(handler);
    }


    public void UnregisterPacketHandler<T>(Action<NetworkConnection, T, Channel> handler) where T : struct, IPacket
    {
        ushort key = PacketHelper.GetKey<T>();
        if (_packetHandlers.TryGetValue(key, out PacketHandlerCollection? packetHandler))
            packetHandler.UnregisterHandler(handler);
    }


    /// <summary>
    /// Sends a packet to a connection.
    /// </summary>
    /// <typeparam name="T">Type of packet to send.</typeparam>
    /// <param name="packet">The packet to send.</param>
    /// <param name="channel">Channel to send on.</param>
    public void SendPacketToServer<T>(T packet, Channel channel = Channel.Reliable) where T : struct, IPacket
    {
        if (Connection == null || !Connection.IsActive)
        {
            Logger.Error("Local connection is not active / does not exist, cannot send packets.");
            return;
        }

        Transport.SendToClient(channel, packet, Connection.ClientId);
    }
}