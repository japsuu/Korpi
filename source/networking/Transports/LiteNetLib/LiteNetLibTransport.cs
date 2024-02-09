using System.Runtime.CompilerServices;
using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;
using Korpi.Networking.Transports.LiteNetLib.Sockets.Client;
using Korpi.Networking.Transports.LiteNetLib.Sockets.Server;
using LiteNetLib.Layers;

namespace Korpi.Networking.Transports.LiteNetLib;

public class LiteNetLibTransport : Transport
{
    ~LiteNetLibTransport()
    {
        Shutdown();
    }


    public override void Initialize(NetworkManager networkManager)
    {
        networkManager.Update += OnUpdate;
    }


    /// <summary>
    /// While true, forces sockets to send data directly to interface without routing.
    /// </summary>
    private bool _dontRoute;

    /// <summary>
    /// Maximum transmission unit for the unreliable channel.
    /// </summary>
    private int _unreliableMtu = 1023;

    /// <summary>
    /// IPv4 address to bind server to.
    /// </summary>
    private string _ipv4BindAddress;

    /// <summary>
    /// Port to use.
    /// </summary>
    private ushort _port = 7770;

    /// <summary>
    /// Maximum number of players which may be connected at once.
    /// </summary>
    private int _maximumClients = 4095;

    /// <summary>
    /// Address to connect.
    /// </summary>
    private string _clientAddress = "localhost";

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


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        Shutdown();
        NetworkManager.Update -= OnUpdate;
    }


    /// <summary>
    /// Gets the address of a remote connection Id.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public override string GetConnectionAddress(int connectionId) => _server.GetConnectionAddress(connectionId);


    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    public override event Action<ClientConnectionStateArgs>? ClientConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for the local server.
    /// </summary>
    public override event Action<ServerConnectionStateArgs>? ServerConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    public override event Action<RemoteConnectionStateArgs>? RemoteConnectionStateChanged;


    /// <summary>
    /// Gets the current local ConnectionState.
    /// </summary>
    /// <param name="server">True if getting ConnectionState for the server.</param>
    public override LocalConnectionState GetConnectionState(bool server) => server ? _server.GetConnectionState() : _client.GetConnectionState();


    /// <summary>
    /// Gets the current ConnectionState of a remote client on the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get ConnectionState for.</param>
    public override RemoteConnectionState GetConnectionState(int connectionId) => _server.GetConnectionState(connectionId);


    /// <summary>
    /// Handles a ConnectionStateArgs for the local client.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
    {
        ClientConnectionStateChanged?.Invoke(connectionStateArgs);
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for the local server.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
    {
        ServerConnectionStateChanged?.Invoke(connectionStateArgs);
        UpdateTimeout();
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for a remote client.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
    {
        RemoteConnectionStateChanged?.Invoke(connectionStateArgs);
    }


    /// <summary>
    /// Called every update to poll for data.
    /// </summary>
    private void OnUpdate()
    {
        _server.PollSocket();
        _client.PollSocket();
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


    /// <summary>
    /// Called when client receives data.
    /// </summary>
    public override event Action<ClientReceivedDataArgs>? ClientReceivedPacket;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedDataArgs"></param>
    public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
    {
        ClientReceivedPacket?.Invoke(receivedDataArgs);
    }


    /// <summary>
    /// Called when server receives data.
    /// </summary>
    public override event Action<ServerReceivedPacketArgs>? ServerReceivedPacket;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedPacketArgs"></param>
    public override void HandleServerReceivedDataArgs(ServerReceivedPacketArgs receivedPacketArgs)
    {
        ServerReceivedPacket?.Invoke(receivedPacketArgs);
    }


    /// <summary>
    /// Sends to the server or all clients.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="packet"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SendToServer(Channel channel, IPacket packet)
    {
        _client.SendToServer(channel, packet);
    }


    /// <summary>
    /// Sends data to a client.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="packet"></param>
    /// <param name="connectionId"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SendToClient(Channel channel, IPacket packet, int connectionId)
    {
        _server.SendToClient(channel, packet, connectionId);
    }


    /// <summary>
    /// Sets which PacketLayer to use with LiteNetLib.
    /// </summary>
    /// <param name="packetLayer"></param>
    public void SetPacketLayer(PacketLayerBase packetLayer)
    {
        _packetLayer = packetLayer;
        if (GetConnectionState(true) != LocalConnectionState.Stopped)
            NetworkManager.Logger.Warn("PacketLayer is set but will not be applied until the server stops.");
        if (GetConnectionState(false) != LocalConnectionState.Stopped)
            NetworkManager.Logger.Warn("PacketLayer is set but will not be applied until the client stops.");

        _server.Initialize(this, _unreliableMtu, _packetLayer, _dontRoute);
        _client.Initialize(this, _unreliableMtu, _packetLayer, _dontRoute);
    }


    public override float GetTimeout(bool asServer) =>

        //Server and client uses the same timeout.
        MAX_TIMEOUT_SECONDS;


    public override void SetTimeout(float value, bool asServer)
    {
    }


    public override int GetMaximumClients() => _server.GetMaximumClients();


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
    /// <param name="address">The address to bind to.</param>
    public override void SetServerBindAddress(string address)
    {
        _ipv4BindAddress = address;
    }


    /// <summary>
    /// Gets which address the server will bind to.
    /// </summary>
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
    public override ushort GetPort()
    {
        //Server.
        ushort? result = _server.GetPort();
        if (result.HasValue)
            return result.Value;

        //Client.
        result = _client.GetPort();
        if (result.HasValue)
            return result.Value;

        return _port;
    }


    /// <summary>
    /// Starts the local server or client using configured settings.
    /// </summary>
    /// <param name="server">True to start server.</param>
    public override bool StartConnection(bool server) => server ? StartServer() : StartClient(_clientAddress);


    /// <summary>
    /// Stops the local server or client.
    /// </summary>
    /// <param name="server">True to stop server.</param>
    public override bool StopConnection(bool server) => server ? StopServer() : StopClient();


    /// <summary>
    /// Stops a remote client from the server, disconnecting the client.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    public override bool StopConnection(int connectionId) => _server.StopConnection(connectionId);


    /// <summary>
    /// Stops both client and server.
    /// </summary>
    public override void Shutdown()
    {
        //Stops client then server connections.
        StopConnection(false);
        StopConnection(true);
    }


    /// <summary>
    /// Starts server.
    /// </summary>
    private bool StartServer()
    {
        _server.Initialize(this, _unreliableMtu, _packetLayer, _dontRoute);
        UpdateTimeout();
        return _server.StartConnection(_port, _maximumClients, _ipv4BindAddress);
    }


    /// <summary>
    /// Stops server.
    /// </summary>
    private bool StopServer() => _server.StopConnection();


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
        const int timeout = MAX_TIMEOUT_SECONDS;
        _client.UpdateTimeout(timeout);
        _server.UpdateTimeout(timeout);
    }


    /// <summary>
    /// Stops the client.
    /// </summary>
    private bool StopClient() => _client.StopConnection();


    /// <summary>
    /// Gets the MTU for a channel. This should take header size into consideration.
    /// For example, if MTU is 1200 and a packet header for this channel is 10 in size, this method should return 1190.
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public override int GetMTU(byte channel) => _unreliableMtu;
}