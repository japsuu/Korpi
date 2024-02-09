using Common.Logging;
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
    private readonly NetworkManager _netManager;
    private readonly TransportManager _transportManager;
    
    /// <summary>
    /// NetworkConnection of the local client.
    /// </summary>
    public NetworkConnection? Connection;
    
    /// <summary>
    /// True if the local client is connected to the server.
    /// </summary>
    public bool Started { get; private set; }

    /// <summary>
    /// All currently connected clients (peers) by clientId.
    /// </summary>
    public readonly Dictionary<int, NetworkConnection> Clients = new();

    /// <summary>
    /// Called after local client has authenticated.
    /// </summary>
    public event Action? Authenticated;
    
    /// <summary>
    /// Called after the local client connection state changes.
    /// </summary>
    public event Action<ClientConnectionStateArgs>? ClientConnectionStateChanged;
    
    /// <summary>
    /// Called when a client other than self connects.
    /// </summary>
    public event Action<RemoteConnectionStateArgs>? RemoteConnectionStateChanged;
    
    /// <summary>
    /// Called when we receive a list of all connected clients from the server (usually right after connecting).
    /// </summary>
    public event Action<ConnectedClientsListArgs>? ReceivedConnectedClientsList;
    
    
    /// <summary>
    /// Creates a new client manager using the specified transport.
    /// </summary>
    /// <param name="netManager">The network manager owning this client manager.</param>
    /// <param name="transportManager">The transport to use.</param>
    public NetClientManager(NetworkManager netManager, TransportManager transportManager)
    {
        _netManager = netManager;
        _transportManager = transportManager;
        _transportManager.Transport.LocalClientReceivedPacket += OnLocalClientReceivePacket;
        _transportManager.Transport.LocalClientConnectionStateChanged += OnLocalClientConnectionStateChanged;
        
        // Listen for other clients connections from server.
        RegisterPacketHandler<ClientConnectionChangePacket>(OnReceiveClientConnectionPacket);
        RegisterPacketHandler<ConnectedClientsPacket>(OnReceiveConnectedClientsPacket);
        RegisterPacketHandler<WelcomePacket>(OnReceiveWelcomePacket);
    }


    /// <summary>
    /// Called when the server sends a welcome packet to the client.
    /// </summary>
    /// <param name="packet">The packet containing the welcome information.</param>
    /// <param name="channel">The channel the packet was received on.</param>
    private void OnReceiveWelcomePacket(WelcomePacket packet, Channel channel)
    {
        // The ClientConnectionChangePacket and ConnectedClientsPacket should have already been received, so we can assume Clients contains this client too.
        ushort clientId = packet.ClientId;
        if (!Clients.TryGetValue(clientId, out Connection))
        {
            // This should never happen unless the connection is dropping and the ClientConnectionChangePacket is lost (or arrives late).
            Logger.Warn(
                "Local client connection could not be found while receiving the Welcome packet." +
                "This can occur if the client is receiving a packet immediately before losing connection.");
            Connection = new NetworkConnection(_netManager, clientId, false);
        }
        
        Logger.Info($"Received welcome packet from server. Assigned clientId is {clientId}.");
        
        // Mark local connection as authenticated.
        Connection.SetAuthenticated();
        Authenticated?.Invoke();
    }


    /// <summary>
    /// Called when a new client connects or disconnects.
    /// </summary>
    /// <param name="packet">The packet containing the connection change information.</param>
    /// <param name="channel">The channel the packet was received on.</param>
    private void OnReceiveClientConnectionPacket(ClientConnectionChangePacket packet, Channel channel)
    {
        bool isNewConnection = packet.Connected;
        int clientId = packet.ClientId;
        RemoteConnectionStateArgs rcs = new RemoteConnectionStateArgs(isNewConnection ? RemoteConnectionState.Started : RemoteConnectionState.Stopped, clientId);

        // If a new connection, invoke event after adding conn to clients, otherwise invoke event before conn is removed from clients.
        if (isNewConnection)
        {
            Clients[clientId] = new NetworkConnection(_netManager, clientId, false);
            RemoteConnectionStateChanged?.Invoke(rcs);
        }
        else
        {
            RemoteConnectionStateChanged?.Invoke(rcs);
            if (!Clients.TryGetValue(clientId, out NetworkConnection? c))
                return;
            
            c.Dispose();
            Clients.Remove(clientId);
        }
    }


    /// <summary>
    /// Called when the server sends a list of all connected clients to the client.
    /// </summary>
    /// <param name="packet">The packet containing the list of connected clients.</param>
    /// <param name="channel">The channel the packet was received on.</param>
    private void OnReceiveConnectedClientsPacket(ConnectedClientsPacket packet, Channel channel)
    {
        NetworkManager.ClearClientsCollection(Clients);

        List<int>? collection = packet.ClientIds;
        if (collection == null)
        {
            // There were no connected clients, technically not possible since the list should contain at least the local client.
            collection = new List<int>();
            Logger.Warn("Received a ConnectedClientsPacket with no connected clients.");
        }
        else
        {
            // There were connected clients, create NetworkConnection objects for them.
            int count = collection.Count;
            for (int i = 0; i < count; i++)
            {
                int id = collection[i];
                Clients[id] = new NetworkConnection(_netManager, id, false);
            }
        }

        ReceivedConnectedClientsList?.Invoke(new ConnectedClientsListArgs(collection));
    }


    /// <summary>
    /// Called when the client receives a packet from the server.
    /// </summary>
    /// <param name="args">The packet and channel received.</param>
    private void OnLocalClientReceivePacket(ClientReceivedPacketArgs args)
    {
        IPacket packet = args.Packet;
        ushort key = packet.GetKey();
        
        if (!_packetHandlers.TryGetValue(key, out PacketHandlerCollection? packetHandler))
        {
            Logger.Warn($"Received a packet of type {packet.GetType().Name} but no handler is registered for it. Ignoring.");
            return;
        }
        
        packetHandler.InvokeHandlers(packet, args.Channel);
    }


    /// <summary>
    /// Called when the local client connection state changes.
    /// </summary>
    /// <param name="args">The new connection state.</param>
    private void OnLocalClientConnectionStateChanged(ClientConnectionStateArgs args)
    {
        LocalConnectionState state = args.ConnectionState;
        Started = state == LocalConnectionState.Started;

        if (!Started)
        {
            Connection = null;
            NetworkManager.ClearClientsCollection(Clients);
        }

        string tName = _transportManager.Transport.GetType().Name;
        string socketInformation = string.Empty;
        if (state == LocalConnectionState.Starting)
            socketInformation = $" Server IP is {_transportManager.GetClientAddress()}, port is {_transportManager.GetPort()}.";
        Logger.Info($"Local client is {state.ToString().ToLower()} for {tName}.{socketInformation}");

        ClientConnectionStateChanged?.Invoke(args);
    }


    /// <summary>
    /// Connects to the server at the specified address and port.
    /// </summary>
    /// <param name="address">The address of the server.</param>
    /// <param name="port">The port of the server.</param>
    public void Connect(string address, ushort port)
    {
        _transportManager.SetClientAddress(address);
        _transportManager.SetPort(port);
        _transportManager.StartConnection(false);
    }
    
    
    /// <summary>
    /// Disconnects from the currently connected server.
    /// </summary>
    public void Disconnect()
    {
        _transportManager.StopConnection(false);
    }


    /// <summary>
    /// Registers a method to call when a packet of the specified type arrives.
    /// </summary>
    /// <param name="handler">Method to call.</param>
    /// <typeparam name="T"></typeparam>
    public void RegisterPacketHandler<T>(Action<T, Channel> handler) where T : struct, IPacket
    {
        ushort key = PacketHelper.GetKey<T>();

        if (!_packetHandlers.TryGetValue(key, out PacketHandlerCollection? packetHandler))
        {
            packetHandler = new ServerPacketHandler<T>();
            _packetHandlers.Add(key, packetHandler);
        }

        packetHandler.RegisterHandler(handler);
    }


    /// <summary>
    /// Unregisters a method from being called when a packet of the specified type arrives.
    /// </summary>
    /// <param name="handler">The method to unregister.</param>
    /// <typeparam name="T">Type of packet to unregister.</typeparam>
    public void UnregisterPacketHandler<T>(Action<T, Channel> handler) where T : struct, IPacket
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
        if (!Started)
        {
            Logger.Error($"Local connection is not started, cannot send packet of type {packet.GetType().Name}.");
            return; 
        }

        _transportManager.SendToServer(channel, packet);
    }
}