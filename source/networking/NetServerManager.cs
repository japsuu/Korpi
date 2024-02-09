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
/// Manages the network server.
/// Does not deal with game logic, only with network communication.
/// </summary>
public class NetServerManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetServerManager));
    private readonly Dictionary<ushort, PacketHandlerCollection> _packetHandlers = new(); // Registered packet handlers by packet type.
    private readonly NetworkManager _netManager;
    private readonly TransportManager _transportManager;
    private Authenticator? _authenticator;

    /// <summary>
    /// True if the server connection has started.
    /// </summary>
    public bool Started { get; private set; }

    /// <summary>
    /// Authenticated and non-authenticated connected clients, by clientId.
    /// </summary>
    public readonly Dictionary<int, NetworkConnection> Clients = new();

    /// <summary>
    /// Called after the local server connection state changes.
    /// </summary>
    public event Action<ServerConnectionStateArgs>? ServerConnectionStateChanged;

    /// <summary>
    /// Called when authenticator has concluded a result for a connection. Boolean is true if authentication passed, false if failed.
    /// </summary>
    public event Action<NetworkConnection, bool>? AuthenticationResultReceived;

    /// <summary>
    /// Called when a remote client state changes with the server.
    /// </summary>
    public event Action<NetworkConnection, RemoteConnectionStateArgs>? RemoteConnectionStateChanged;

    /// <summary>
    /// Called when a client is removed from the server using Kick. This is invoked before the client is disconnected.
    /// NetworkConnection (when available), clientId, and KickReason are provided.
    /// </summary>
    public event Action<NetworkConnection?, int, KickReason>? ClientKicked;


    /// <summary>
    /// Creates a new server manager using the specified transport.
    /// </summary>
    /// <param name="netManager">The network manager owning this server manager.</param>
    /// <param name="transportManager">The transport to use.</param>
    public NetServerManager(NetworkManager netManager, TransportManager transportManager)
    {
        _netManager = netManager;
        _transportManager = transportManager;
        _transportManager.Transport.LocalServerReceivedPacket += OnLocalServerReceivePacket;
        _transportManager.Transport.LocalServerConnectionStateChanged += OnLocalServerConnectionStateChanged;
        _transportManager.Transport.RemoteClientConnectionStateChanged += OnRemoteClientConnectionStateChanged;
    }


    /// <summary>
    /// Sets the maximum number of connections the server can have.
    /// </summary>
    public void SetMaxConnections(int maxConnections)
    {
        _transportManager.SetMaximumClients(maxConnections);
    }


    /// <summary>
    /// Assigns an authenticator to the server.
    /// </summary>
    /// <param name="authenticator">The authenticator to use.</param>
    public void SetAuthenticator(Authenticator? authenticator)
    {
        _authenticator = authenticator;
        if (_authenticator == null)
            return;
        
        _authenticator.Initialize(_netManager);
        _authenticator.ConcludedAuthenticationResult += OnAuthenticatorConcludeResult;
    }


    /// <summary>
    /// Starts the server with the specified address and port.
    /// </summary>
    /// <param name="address">The address to bind to.</param>
    /// <param name="port">The port to bind to.</param>
    public void StartServer(string address, ushort port)
    {
        _transportManager.SetServerBindAddress(address);
        _transportManager.SetPort(port);
        _transportManager.StartConnection(true);
    }


    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <param name="sendDisconnectionPackets">True to send a disconnect packet to all clients before stopping the server.</param>
    public void StopServer(bool sendDisconnectionPackets)
    {
        if (sendDisconnectionPackets)
            SendDisconnectPackets(Clients.Values.ToList(), true);

        _transportManager.StopConnection(true);
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


    /// <summary>
    /// Unregisters a method call from a packet type.
    /// </summary>
    /// <param name="handler">Method to unregister.</param>
    /// <typeparam name="T">Type of packet being unregistered.</typeparam>
    public void UnregisterPacketHandler<T>(Action<NetworkConnection, T, Channel> handler) where T : struct, IPacket
    {
        ushort key = PacketHelper.GetKey<T>();
        if (_packetHandlers.TryGetValue(key, out PacketHandlerCollection? packetHandler))
            packetHandler.UnregisterHandler(handler);
    }


    /// <summary>
    /// Sends a packet to a connection.
    /// </summary>
    /// <param name="connection">Connection to send to.</param>
    /// <param name="message">Packet data being sent.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to receive this packet.</param>
    /// <param name="channel">Channel to send on.</param>
    /// <typeparam name="T">Type of packet to send.</typeparam>
    public void SendPacketToClient<T>(NetworkConnection connection, T message, bool requireAuthenticated = true, Channel channel = Channel.Reliable)
        where T : struct, IPacket
    {
        if (!Started)
        {
            Logger.Warn("Cannot send packet to client because server is not active.");
            return;
        }

        if (!connection.IsActive)
        {
            Logger.Warn("Connection is not active, cannot send packet.");
            return;
        }

        if (requireAuthenticated && !connection.IsAuthenticated)
        {
            Logger.Warn($"Cannot send packet of type {typeof(T).Name} to client {connection.ClientId} because they are not authenticated.");
            return;
        }

        _transportManager.SendToClient(channel, message, connection.ClientId);
    }


    /// <summary>
    /// Sends a packet to all clients.
    /// </summary>
    /// <param name="message">Packet data being sent.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to receive this packet.</param>
    /// <param name="channel">Channel to send on.</param>
    /// <typeparam name="T">The type of packet to send.</typeparam>
    public void SendPacketToAllClients<T>(T message, bool requireAuthenticated = true, Channel channel = Channel.Reliable) where T : struct, IPacket
    {
        if (!Started)
        {
            Logger.Warn("Cannot send packet to clients because server is not active.");
            return;
        }

        foreach (NetworkConnection c in Clients.Values)
            SendPacketToClient(c, message, requireAuthenticated, channel);
    }


    /// <summary>
    /// Sends a packet to all clients except the specified one.
    /// </summary>
    public void SendPacketToAllClientsExcept<T>(T message, NetworkConnection except, bool requireAuthenticated = true, Channel channel = Channel.Reliable)
        where T : struct, IPacket
    {
        if (!Started)
        {
            Logger.Warn("Cannot send packet to clients because server is not active.");
            return;
        }

        foreach (NetworkConnection c in Clients.Values)
        {
            if (c == except)
                continue;
            SendPacketToClient(c, message, requireAuthenticated, channel);
        }
    }


    /// <summary>
    /// Sends a packet to all clients except the specified ones.
    /// </summary>
    public void SendPacketToAllClientsExcept<T>(T message, List<NetworkConnection> except, bool requireAuthenticated = true, Channel channel = Channel.Reliable)
        where T : struct, IPacket
    {
        if (!Started)
        {
            Logger.Warn("Cannot send packet to clients because server is not active.");
            return;
        }

        foreach (NetworkConnection c in Clients.Values)
        {
            if (except.Contains(c))
                continue;
            SendPacketToClient(c, message, requireAuthenticated, channel);
        }
    }


    /// <summary>
    /// Kicks a connection immediately while invoking ClientKicked.
    /// </summary>
    /// <param name="conn">Client to kick.</param>
    /// <param name="kickReason">Reason client is being kicked.</param>
    public void Kick(NetworkConnection conn, KickReason kickReason)
    {
        if (!conn.IsValid)
            return;

        ClientKicked?.Invoke(conn, conn.ClientId, kickReason);
        if (conn.IsActive)
            conn.Disconnect(true);
    }


    /// <summary>
    /// Kicks a connection immediately while invoking ClientKicked.
    /// </summary>
    /// <param name="connId">Id of the client to kick.</param>
    /// <param name="kickReason">Reason client is being kicked.</param>
    public void Kick(int connId, KickReason kickReason)
    {
        ClientKicked?.Invoke(null, connId, kickReason);

        _transportManager.StopConnection(connId, true);
    }


    /// <summary>
    /// Handles a received packet.
    /// </summary>
    /// <param name="args"></param>
    private void OnLocalServerReceivePacket(ServerReceivedPacketArgs args)
    {
        // Not from a valid connection.
        if (args.ConnectionId < 0)
        {
            Logger.Warn($"Received a packet from an unknown connection with id {args.ConnectionId}. Ignoring.");
            return;
        }

        IPacket packet = args.Packet;
        if (!Clients.TryGetValue(args.ConnectionId, out NetworkConnection? connection))
        {
            Logger.Warn($"ConnectionId {args.ConnectionId} not found within Clients. Connection will be kicked immediately.");
            Kick(args.ConnectionId, KickReason.UnexpectedProblem);
            return;
        }

        //TODO: Kick the client immediately if packet is over MTU.

        ushort key = packet.GetKey();

        if (!_packetHandlers.TryGetValue(key, out PacketHandlerCollection? packetHandler))
        {
            Logger.Warn($"Received a packet of type {packet.GetType().Name} but no handler is registered for it. Ignoring.");
            return;
        }

        if (packetHandler.RequireAuthentication && !connection.IsAuthenticated)
        {
            Logger.Warn($"Client {connection.ClientId} sent a packet of type {packet.GetType().Name} without being authenticated. Kicking.");
            Kick(connection, KickReason.ExploitAttempt);
            return;
        }

        packetHandler.InvokeHandlers(connection, packet, args.Channel);
    }


    /// <summary>
    /// Called when the local server connection state changes.
    /// </summary>
    /// <param name="args"></param>
    private void OnLocalServerConnectionStateChanged(ServerConnectionStateArgs args)
    {
        LocalConnectionState state = args.ConnectionState;
        Started = state == LocalConnectionState.Started;

        if (!Started)
        {
            NetworkManager.ClearClientsCollection(Clients);
        }

        string tName = _transportManager.Transport.GetType().Name;
        string socketInformation = string.Empty;
        if (state == LocalConnectionState.Starting)
            socketInformation = $" Listening on port {_transportManager.GetPort()}.";
        Logger.Info($"Local server is {state.ToString().ToLower()} for {tName}.{socketInformation}");

        ServerConnectionStateChanged?.Invoke(args);
    }


    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    private void OnRemoteClientConnectionStateChanged(RemoteConnectionStateArgs args)
    {
        int id = args.ConnectionId;
        if (id is < 0 or > short.MaxValue)
        {
            Logger.Error($"Received an invalid connection id {id} from transport. Kicking client.");
            Kick(args.ConnectionId, KickReason.UnexpectedProblem);
            return;
        }

        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Started:
            {
                Logger.Info($"Remote connection started for clientId {id}.");
                NetworkConnection conn = new(_netManager, id, true);
                Clients.Add(args.ConnectionId, conn);
                RemoteConnectionStateChanged?.Invoke(conn, args);

                // Connection is no longer valid. This can occur if the user changes the state using the RemoteClientConnectionStateChanged event.
                if (!conn.IsValid)
                    return;

                if (_authenticator != null)
                    _authenticator.OnRemoteConnection(conn);
                else
                    ClientAuthenticated(conn);
                break;
            }
            case RemoteConnectionState.Stopped:
            {
                /* If client's connection is found then clean
                 * them up from server. */
                if (Clients.TryGetValue(id, out NetworkConnection? conn))
                {
                    conn.SetDisconnecting(true);
                    RemoteConnectionStateChanged?.Invoke(conn, args);
                    Clients.Remove(id);
                    SendClientConnectionChangePacket(false, conn);

                    conn.Dispose();
                    Logger.Info($"Remote connection stopped for clientId {id}.");
                }

                break;
            }
        }
    }


    /// <summary>
    /// Called when the authenticator has concluded a result for a connection.
    /// </summary>
    /// <param name="conn">The connection that was authenticated.</param>
    /// <param name="success">True if authentication passed, false if failed.</param>
    private void OnAuthenticatorConcludeResult(NetworkConnection conn, bool success)
    {
        if (success)
            ClientAuthenticated(conn);
        else
            conn.Disconnect(false);
    }


    /// <summary>
    /// Called when a remote client authenticates with the server.
    /// </summary>
    private void ClientAuthenticated(NetworkConnection connection)
    {
        // Immediately send connectionId to client.
        connection.SetAuthenticated();
        /* Send client Ids before telling the client
         * they are authenticated. This is important because when the client becomes
         * authenticated they set their LocalConnection using Clients field in ClientManager,
         * which is set after getting Ids. */
        SendClientConnectionChangePacket(true, connection);
        SendWelcomePacket(connection);

        AuthenticationResultReceived?.Invoke(connection, true);
    }


    /// <summary>
    /// Sends a welcome packet to a client.
    /// </summary>
    /// <param name="connection"></param>
    private void SendWelcomePacket(NetworkConnection connection)
    {
        // Sanity check.
        if (!connection.IsValid)
        {
            Logger.Warn("Cannot send welcome packet to client because connection is not valid.");
            return;
        }

        WelcomePacket welcome = new((ushort)connection.ClientId);
        _transportManager.SendToClient(Channel.Reliable, welcome, connection.ClientId);
    }


    /// <summary>
    /// Sends a disconnect message to all clients, and optionally immediately iterates outgoing packets to ensure they are sent.
    /// </summary>
    private void SendDisconnectPackets(List<NetworkConnection> conns, bool iterate)
    {
        DisconnectPacket packet = new();

        // Send packet to each client, authenticated or not.
        foreach (NetworkConnection c in conns)
            SendPacketToClient(c, packet, false);

        if (iterate)
            _transportManager.IterateOutgoing(true);
    }


    /// <summary>
    /// Sends a client connection state change to owner and other clients if applicable.
    /// </summary>
    private void SendClientConnectionChangePacket(bool connected, NetworkConnection conn)
    {
        // Send a packet to all authenticated clients with the clientId that just connected.
        // It is important that the just connected client will also get this, so that they can later successfully get a reference to their own connection.
        ClientConnectionChangePacket changeMsg = new(conn.ClientId, connected);

        foreach (NetworkConnection c in Clients.Values.Where(c => c.IsAuthenticated))
            SendPacketToClient(c, changeMsg);

        // If this was a new connection, the new client must also receive all currently connected client ids.
        if (!connected)
            return;

        //Send already connected clients to the connection that just joined.
        List<int>? clientIds = Clients.Count > 0 ? Clients.Keys.ToList() : null;
        ConnectedClientsPacket allIdsPacket = new(clientIds);
        SendPacketToClient(conn, allIdsPacket);
    }
}