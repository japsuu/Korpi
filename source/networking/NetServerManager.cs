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
    private readonly Dictionary<ushort, PacketHandlerCollection> _packetHandlers = new();
    private Transport Transport { get; }
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
    public event Action<ServerConnectionStateArgs>? OnServerConnectionState;

    /// <summary>
    /// Called when authenticator has concluded a result for a connection. Boolean is true if authentication passed, false if failed.
    /// </summary>
    public event Action<NetworkConnection, bool>? OnAuthenticationResult;

    /// <summary>
    /// Called when a remote client state changes with the server.
    /// </summary>
    public event Action<NetworkConnection, RemoteConnectionStateArgs>? OnRemoteConnectionState;

    /// <summary>
    /// Called when a client is removed from the server using Kick. This is invoked before the client is disconnected.
    /// NetworkConnection (when available), clientId, and KickReason are provided.
    /// </summary>
    public event Action<NetworkConnection?, int, KickReason>? OnClientKick;


    public NetServerManager(Transport transport)
    {
        Transport = transport;
        Transport.OnServerReceivedPacket += HandlePacket;
        Transport.OnServerConnectionState += OnServerConnectionStateChanged;
        Transport.OnRemoteConnectionState += OnRemoteConnectionStateChanged;
    }


    private void OnServerConnectionStateChanged(ServerConnectionStateArgs args)
    {
        Started = Transport.GetConnectionState(true) == LocalConnectionState.Started;
        
        string tName = Transport.GetType().Name;
        string socketInformation = string.Empty;
        if (args.ConnectionState == LocalConnectionState.Starting)
            socketInformation = $" Listening on port {Transport.GetPort()}.";
        Logger.Info($"Local server is {args.ConnectionState.ToString().ToLower()} for {tName}.{socketInformation}");

        OnServerConnectionState?.Invoke(args);
    }


    public void SetMaxConnections(int maxConnections)
    {
        Transport.SetMaximumClients(maxConnections);
    }


    public void SetAuthenticator(Authenticator? authenticator)
    {
        _authenticator = authenticator;
        if (_authenticator != null)
        {
            _authenticator.OnAuthenticationResult += OnAuthenticationResultReceived;
        }
    }


    private void OnAuthenticationResultReceived(NetworkConnection conn, bool success)
    {
        if (!success)
            conn.Disconnect(false);
        else
            ClientAuthenticated(conn);
    }


    public void StartServer(string address, ushort port)
    {
        Transport.SetServerBindAddress(address);
        Transport.SetPort(port);
        Transport.StartConnection(true);
    }


    public void StopServer()
    {
        Transport.StopConnection(true);
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
    /// <param name="connection">Connection to send to.</param>
    /// <param name="message">Packet data being sent.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to receive this packet.</param>
    /// <param name="channel">Channel to send on.</param>
    public void SendPacketToClient<T>(NetworkConnection connection, T message, bool requireAuthenticated = true, Channel channel = Channel.Reliable) where T : struct, IPacket
    {
        if (!Started)
        {
            Logger.Warn("Cannot send broadcast to client because server is not active.");
            return;
        }
        if (!connection.IsActive)
        {
            Logger.Warn("Connection is not active, cannot send broadcast.");
            return;
        }

        if (requireAuthenticated && !connection.IsAuthenticated)
        {
            Logger.Warn($"Cannot send packet of type {typeof(T).Name} to client {connection.ClientId} because they are not authenticated.");
            return;
        }
        
        Transport.SendToClient(channel, message, connection.ClientId);
    }


    /// <summary>
    /// Kicks a connection immediately while invoking OnClientKick.
    /// </summary>
    /// <param name="conn">Client to kick.</param>
    /// <param name="kickReason">Reason client is being kicked.</param>
    public void Kick(NetworkConnection conn, KickReason kickReason)
    {
        if (!conn.IsValid)
            return;

        OnClientKick?.Invoke(conn, conn.ClientId, kickReason);
        if (conn.IsActive)
            conn.Disconnect(true);
    }


    /// <summary>
    /// Kicks a connection immediately while invoking OnClientKick.
    /// </summary>
    /// <param name="connId">Id of the client to kick.</param>
    /// <param name="kickReason">Reason client is being kicked.</param>
    public void Kick(int connId, KickReason kickReason)
    {
        OnClientKick?.Invoke(null, connId, kickReason);

        Transport.StopConnection(connId, true);
    }


    private void HandlePacket(ServerReceivedPacketArgs args)
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
    /// Called when a connection state changes for a remote client.
    /// </summary>
    private void OnRemoteConnectionStateChanged(RemoteConnectionStateArgs args)
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
                NetworkConnection conn = new(this, Transport, id, true);
                Clients.Add(args.ConnectionId, conn);
                OnRemoteConnectionState?.Invoke(conn, args);

                // Connection is no longer valid. This can occur if the user changes the state using the OnRemoteConnectionState event.
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
                    OnRemoteConnectionState?.Invoke(conn, args);
                    Clients.Remove(id);
                    BroadcastClientConnectionChange(false, conn);

                    conn.Dispose();
                    Logger.Info($"Remote connection stopped for clientId {id}.");
                }

                break;
            }
        }
    }


    /// <summary>
    /// Called when a remote client authenticates with the server.
    /// </summary>
    private void ClientAuthenticated(NetworkConnection connection)
    {
        /* Immediately send connectionId to client. Some transports
         * don't give clients their remoteId, therefor it has to be sent
         * by the ServerManager. This packet is very simple and can be built
         * on the spot. */
        connection.SetAuthenticated();
        /* Send client Ids before telling the client
         * they are authenticated. This is important because when the client becomes
         * authenticated they set their LocalConnection using Clients field in ClientManager,
         * which is set after getting Ids. */
        BroadcastClientConnectionChange(true, connection);
        SendWelcomePacket(connection);

        OnAuthenticationResult?.Invoke(connection, true);
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
        Transport.SendToClient(Channel.Reliable, welcome, connection.ClientId);
    }


    /// <summary>
    /// Sends a client connection state change to owner and other clients if applicable.
    /// </summary>
    private void BroadcastClientConnectionChange(bool connected, NetworkConnection conn)
    {
        /* Send a broadcast to all authenticated clients with the clientId
         * that just connected. The conn client will also get this. */
        ClientConnectionChangePacket changeMsg = new ClientConnectionChangePacket(conn.ClientId, connected);
        
        foreach (NetworkConnection c in Clients.Values.Where(c => c.IsAuthenticated))
            SendPacketToClient(c, changeMsg);

        // If this was a new connection, the new client must also receive all currently connected client ids.
        if (!connected)
            return;

        //Send already connected clients to the connection that just joined.
        ConnectedClientsPacket allIdsPacket = new(Clients.Keys.ToList());
        SendPacketToClient(conn, allIdsPacket);
    }
}