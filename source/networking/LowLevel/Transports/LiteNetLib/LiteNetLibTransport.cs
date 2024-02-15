using System.Runtime.CompilerServices;
using Common.Logging;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.LowLevel.Transports.EventArgs;
using Korpi.Networking.LowLevel.Transports.LiteNetLib.Core;
using LiteNetLib.Layers;

namespace Korpi.Networking.LowLevel.Transports.LiteNetLib;

public class LiteNetLibTransport : Transport
{
    internal static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(LiteNetLibTransport));


    ~LiteNetLibTransport()
    {
        Shutdown();
    }


    #region Serialized.

    /* Settings / Misc. */
    /// <summary>
    /// While true, forces sockets to send data directly to interface without routing.
    /// </summary>
    private bool _dontRoute;

    /* Channels. */
    /// <summary>
    /// Maximum transmission unit for the unreliable channel.
    /// </summary>
    private int _unreliableMtu = 1023;

    /* Server. */
    /// <summary>
    /// IPv4 address to bind server to.
    /// </summary>
    private string _ipv4BindAddress;

    /// <summary>
    /// Enable IPv6 only on demand to avoid problems in Linux environments where it may have been disabled on host
    /// </summary>
    private bool _enableIpv6 = true;

    /// <summary>
    /// IPv6 address to bind server to.
    /// </summary>
    private string _ipv6BindAddress;

    /// <summary>
    /// Port to use.
    /// </summary>
    private ushort _port = 7770;

    /// <summary>
    /// Maximum number of players which may be connected at once.
    /// </summary>
    private int _maximumClients = 4095;

    /* Client. */
    /// <summary>
    /// Address to connect.
    /// </summary>
    private string _clientAddress = "localhost";

    #endregion

    #region Private.

    /// <summary>
    /// PacketLayer to use with LiteNetLib.
    /// </summary>
    private PacketLayerBase _packetLayer;

    /// <summary>
    /// Server socket and handler.
    /// </summary>
    private ServerSocket _server = new();

    /// <summary>
    /// Client socket and handler.
    /// </summary>
    private ClientSocket _client = new();

    #endregion

    #region Const.

    /// <summary>
    /// Maximum timeout value to use.
    /// </summary>
    private const ushort MAX_TIMEOUT_SECONDS = 1800;

    /// <summary>
    /// Minimum UDP packet size allowed.
    /// </summary>
    private const int MINIMUM_UDP_MTU = 576;

    /// <summary>
    /// Maximum UDP packet size allowed.
    /// </summary>
    private const int MAXIMUM_UDP_MTU = 1023;

    #endregion


    public override void Initialize(NetworkManager networkManager)
    {
        base.Initialize(networkManager);
        networkManager.Update += UpdateSockets;
    }


    #region ConnectionStates.

    /// <summary>
    /// Gets the address of a remote connection Id.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public override string GetRemoteConnectionAddress(int connectionId) => _server.GetConnectionAddress(connectionId);


    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    public override event Action<ClientConnectionStateArgs>? LocalClientConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for the local server.
    /// </summary>
    public override event Action<ServerConnectionStateArgs>? LocalServerConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    public override event Action<RemoteConnectionStateArgs>? RemoteClientConnectionStateChanged;


    /// <summary>
    /// Gets the current local ConnectionState.
    /// </summary>
    /// <param name="server">True if getting ConnectionState for the server.</param>
    public override LocalConnectionState GetLocalConnectionState(bool server)
    {
        if (server)
            return _server.GetConnectionState();
        else
            return _client.GetConnectionState();
    }


    /// <summary>
    /// Gets the current ConnectionState of a remote client on the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get ConnectionState for.</param>
    public override RemoteConnectionState GetRemoteConnectionState(int connectionId) => _server.GetConnectionState(connectionId);


    /// <summary>
    /// Handles a ConnectionStateArgs for the local client.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
    {
        LocalClientConnectionStateChanged?.Invoke(connectionStateArgs);
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for the local server.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
    {
        LocalServerConnectionStateChanged?.Invoke(connectionStateArgs);
        UpdateTimeout();
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for a remote client.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
    {
        RemoteClientConnectionStateChanged?.Invoke(connectionStateArgs);
    }

    #endregion

    #region Iterating.

    /// <summary>
    /// Called every update to poll for data.
    /// </summary>
    private void UpdateSockets()
    {
        _server?.PollSocket();
        _client?.PollSocket();
    }


    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="server">True to process data received on the server.</param>
    public override void IterateIncoming(bool server)
    {
        if (server)
            _server.IterateIncoming();
        else
            _client.IterateIncoming();
    }


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="server">True to process data received on the server.</param>
    public override void IterateOutgoing(bool server)
    {
        if (server)
            _server.IterateOutgoing();
        else
            _client.IterateOutgoing();
    }

    #endregion

    #region ReceivedData.

    /// <summary>
    /// Called when client receives data.
    /// </summary>
    public override event Action<ClientReceivedDataArgs>? LocalClientReceivedPacket;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedDataArgs"></param>
    public void HandleClientReceivedPacketArgs(ClientReceivedDataArgs receivedDataArgs)
    {
        LocalClientReceivedPacket?.Invoke(receivedDataArgs);
    }


    /// <summary>
    /// Called when server receives data.
    /// </summary>
    public override event Action<ServerReceivedDataArgs>? ServerReceivedPacket;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedDataArgs"></param>
    public void HandleServerReceivedPacketArgs(ServerReceivedDataArgs receivedDataArgs)
    {
        ServerReceivedPacket?.Invoke(receivedDataArgs);
    }

    #endregion

    #region Sending.

    /// <summary>
    /// Sends to the server or all clients.
    /// </summary>
    /// <param name="channelId">Channel to use.</param>
    /// <param name="segment">Data to send.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SendToServer(Channel channel, ArraySegment<byte> segment)
    {
        _client.SendToServer(channel, segment);
    }


    /// <summary>
    /// Sends data to a client.
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="segment"></param>
    /// <param name="connectionId"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SendToClient(Channel channel, ArraySegment<byte> segment, int connectionId)
    {
        _server.SendToClient(channel, segment, connectionId);
    }

    #endregion

    #region Configuration.

    /// <summary>
    /// Sets which PacketLayer to use with LiteNetLib.
    /// </summary>
    /// <param name="packetLayer"></param>
    public void SetPacketLayer(PacketLayerBase packetLayer)
    {
        _packetLayer = packetLayer;
        if (GetLocalConnectionState(true) != LocalConnectionState.Stopped)
            Logger.Warn("PacketLayer is set but will not be applied until the server stops.");
        if (GetLocalConnectionState(false) != LocalConnectionState.Stopped)
            Logger.Warn("PacketLayer is set but will not be applied until the client stops.");

        _server.Initialize(this, _unreliableMtu, _packetLayer, _enableIpv6, _dontRoute);
        _client.Initialize(this, _unreliableMtu, _packetLayer, _dontRoute);
    }


    /// <summary>
    /// How long in seconds until either the server or client socket must go without data before being timed out.
    /// </summary>
    /// <param name="asServer">True to get the timeout for the server socket, false for the client socket.</param>
    /// <returns></returns>
    public override float GetTimeout(bool asServer) =>

        //Server and client uses the same timeout.
        MAX_TIMEOUT_SECONDS;


    /// <summary>
    /// Sets how long in seconds until either the server or client socket must go without data before being timed out.
    /// </summary>
    /// <param name="asServer">True to set the timeout for the server socket, false for the client socket.</param>
    public override void SetTimeout(float value, bool asServer)
    {
    }


    /// <summary>
    /// Returns the maximum number of clients allowed to connect to the server. If the transport does not support this method the value -1 is returned.
    /// </summary>
    /// <returns></returns>
    public override int GetMaximumClients() => _server.GetMaximumClients();


    /// <summary>
    /// Sets maximum number of clients allowed to connect to the server. If applied at runtime and clients exceed this value existing clients will stay connected but new clients may not connect.
    /// </summary>
    /// <param name="value"></param>
    public override void SetMaximumClients(int value)
    {
        _maximumClients = value;
        _server.SetMaximumClients(value);
    }


    /// <summary>
    /// Sets which address the client will connect to.
    /// </summary>
    /// <param name="address"></param>
    public override void SetClientAddress(string address)
    {
        _clientAddress = address;
    }


    /// <summary>
    /// Gets which address the client will connect to.
    /// </summary>
    public override string GetClientAddress() => _clientAddress;


    /// <summary>
    /// Sets which address the server will bind to.
    /// </summary>
    /// <param name="address"></param>
    public override void SetServerBindAddress(string address)
    {
        _ipv4BindAddress = address;
    }


    /// <summary>
    /// Gets which address the server will bind to.
    /// </summary>
    /// <param name="address"></param>
    public override string GetServerBindAddress() => _ipv4BindAddress;


    /// <summary>
    /// Sets which port to use.
    /// </summary>
    /// <param name="port"></param>
    public override void SetPort(ushort port)
    {
        _port = port;
    }


    /// <summary>
    /// Gets which port to use.
    /// </summary>
    /// <param name="port"></param>
    public override ushort GetPort()
    {
        //Server.
        ushort? result = _server?.GetPort();
        if (result.HasValue)
            return result.Value;

        //Client.
        result = _client?.GetPort();
        if (result.HasValue)
            return result.Value;

        return _port;
    }

    #endregion

    #region Start and stop.

    /// <summary>
    /// Starts the local server or client using configured settings.
    /// </summary>
    /// <param name="server">True to start server.</param>
    public override bool StartLocalConnection(bool server)
    {
        if (server)
            return StartServer();
        else
            return StartClient(_clientAddress);
    }


    /// <summary>
    /// Stops the local server or client.
    /// </summary>
    /// <param name="server">True to stop server.</param>
    public override bool StopLocalConnection(bool server)
    {
        if (server)
            return StopServer();
        else
            return StopClient();
    }


    /// <summary>
    /// Stops a remote client from the server, disconnecting the client.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    /// <param name="immediately">True to abrutly stop the client socket. The technique used to accomplish immediate disconnects may vary depending on the transport.
    /// When not using immediate disconnects it's recommended to perform disconnects using the ServerManager rather than accessing the transport directly.
    /// </param>
    public override bool StopRemoteConnection(int connectionId, bool immediately) => _server.StopConnection(connectionId);


    /// <summary>
    /// Stops both client and server.
    /// </summary>
    public override void Shutdown()
    {
        //Stops client then server connections.
        StopLocalConnection(false);
        StopLocalConnection(true);
    }


    #region Privates.

    /// <summary>
    /// Starts server.
    /// </summary>
    private bool StartServer()
    {
        _server.Initialize(this, _unreliableMtu, _packetLayer, _enableIpv6, _dontRoute);
        UpdateTimeout();
        return _server.StartConnection(_port, _maximumClients, _ipv4BindAddress, _ipv6BindAddress);
    }


    /// <summary>
    /// Stops server.
    /// </summary>
    private bool StopServer()
    {
        if (_server == null)
            return false;
        else
            return _server.StopConnection();
    }


    /// <summary>
    /// Starts the client.
    /// </summary>
    /// <param name="address"></param>
    private bool StartClient(string address)
    {
        _client.Initialize(this, _unreliableMtu, _packetLayer, _dontRoute);
        UpdateTimeout();
        return _client.StartConnection(address, _port);
    }


    /// <summary>
    /// Updates clients timeout values.
    /// </summary>
    private void UpdateTimeout()
    {
        int timeout = MAX_TIMEOUT_SECONDS;
        _client.UpdateTimeout(timeout);
        _server.UpdateTimeout(timeout);
    }


    /// <summary>
    /// Stops the client.
    /// </summary>
    private bool StopClient()
    {
        if (_client == null)
            return false;
        else
            return _client.StopConnection();
    }

    #endregion

    #endregion
}