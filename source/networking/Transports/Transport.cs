using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;

namespace Korpi.Networking.Transports;

/// <summary>
/// Represents an object that can handle network messages.
/// </summary>
public abstract class Transport : IDisposable
{
    private const string NOT_SUPPORTED_MESSAGE = "The current transport does not support the feature '{0}'.";
    
    /// <summary>
    /// NetworkManager for this transport.
    /// </summary>
    public NetworkManager NetworkManager { get; private set; } = null!;

    #region Connection States

    /// <summary>
    /// Gets the address of a remote connection Id.
    /// </summary>
    /// <param name="connectionId">Connection id to get the address for.</param>
    /// <returns></returns>
    public abstract string GetConnectionAddress(int connectionId);


    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    public abstract event Action<ClientConnectionStateArgs>? OnClientConnectionState;

    /// <summary>
    /// Called when a connection state changes for the local server.
    /// </summary>
    public abstract event Action<ServerConnectionStateArgs>? OnServerConnectionState;

    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    public abstract event Action<RemoteConnectionStateArgs>? OnRemoteConnectionState;


    /// <summary>
    /// Handles a ConnectionStateArgs for the local client.
    /// </summary>
    /// <param name="connectionStateArgs">Data being handled.</param>
    public abstract void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs);


    /// <summary>
    /// Handles a ConnectionStateArgs for the local server.
    /// </summary>
    /// <param name="connectionStateArgs">Data being handled.</param>
    public abstract void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs);


    /// <summary>
    /// Handles a ConnectionStateArgs for a remote client.
    /// </summary>
    /// <param name="connectionStateArgs">Data being handled.</param>
    public abstract void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs);


    /// <summary>
    /// Gets the current local ConnectionState.
    /// </summary>
    /// <param name="server">True if getting ConnectionState for the server.</param>
    public abstract LocalConnectionState GetConnectionState(bool server);


    /// <summary>
    /// Gets the current ConnectionState of a client connected to the server. Can only be called on the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get ConnectionState for.</param>
    public abstract RemoteConnectionState GetConnectionState(int connectionId);

    #endregion

    #region Sending Data

    /// <summary>
    /// Sends to the server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="packet"></param>
    public abstract void SendToServer(Channel channel, IPacket packet);


    /// <summary>
    /// Sends to a client.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="packet"></param>
    /// <param name="connectionId">ConnectionId to send to. When sending to clients can be used to specify which connection to send to.</param>
    public abstract void SendToClient(Channel channel, IPacket packet, int connectionId);

    #endregion

    #region Receiving

    /// <summary>
    /// Called when the client receives data.
    /// </summary>
    public abstract event Action<ClientReceivedDataArgs>? OnClientReceivedData;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedDataArgs">Data being handled.</param>
    public abstract void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs);


    /// <summary>
    /// Called when the server receives data.
    /// </summary>
    public abstract event Action<ServerReceivedPacketArgs>? OnServerReceivedPacket;


    /// <summary>
    /// Handles a ServerReceivedPacketArgs.
    /// </summary>
    /// <param name="receivedPacketArgs">Data being handled.</param>
    public abstract void HandleServerReceivedDataArgs(ServerReceivedPacketArgs receivedPacketArgs);

    #endregion

    #region Iterating.

    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="server">True to process data received on the server.</param>
    public abstract void IterateIncoming(bool server);


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="server">True to process data received on the server.</param>
    public abstract void IterateOutgoing(bool server);

    #endregion

    #region Configuration.

    /// <summary>
    /// Gets how long in seconds until either the server or client socket must go without data before being timed out.
    /// </summary>
    /// <param name="asServer">True to get the timeout for the server socket, false for the client socket.</param>
    /// <returns></returns>
    public virtual float GetTimeout(bool asServer)
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetTimeout));
        return -1f;
    }


    /// <summary>
    /// Sets how long in seconds until either the server or client socket must go without data before being timed out.
    /// </summary>
    /// <param name="value">The new timeout.</param>
    /// <param name="asServer">True to set the timeout for the server socket, false for the client socket.</param>
    public virtual void SetTimeout(float value, bool asServer)
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetTimeout));
    }


    /// <summary>
    /// Returns the maximum number of clients allowed to connect to the server. If the transport does not support this method the value -1 is returned.
    /// </summary>
    /// <returns>Maximum clients transport allows.</returns>
    public virtual int GetMaximumClients()
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetMaximumClients));
        return -1;
    }


    /// <summary>
    /// Sets the maximum number of clients allowed to connect to the server. If applied at runtime and clients exceed this value existing clients will stay connected but new clients may not connect.
    /// </summary>
    /// <param name="value">Maximum clients to allow.</param>
    public virtual void SetMaximumClients(int value)
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetMaximumClients));
    }


    /// <summary>
    /// Sets which address the client will connect to.
    /// </summary>
    /// <param name="address">Address client will connect to.</param>
    public virtual void SetClientAddress(string address)
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetClientAddress));
    }


    /// <summary>
    /// Returns which address the client will connect to.
    /// </summary>
    public virtual string GetClientAddress()
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetClientAddress));
        return string.Empty;
    }


    /// <summary>
    /// Sets which address the server will bind to.
    /// </summary>
    /// <param name="address">Address server will bind to.</param>
    public virtual void SetServerBindAddress(string address)
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetServerBindAddress));
    }


    /// <summary>
    /// Gets which address the server will bind to.
    /// </summary>
    public virtual string GetServerBindAddress()
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetServerBindAddress));
        return string.Empty;
    }


    /// <summary>
    /// Sets which port to use.
    /// </summary>
    /// <param name="port">Port to use.</param>
    public virtual void SetPort(ushort port)
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetPort));
    }


    /// <summary>
    /// Gets which port to use.
    /// </summary>
    public virtual ushort GetPort()
    {
        NetworkManager.Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetPort));
        return 0;
    }

    #endregion

    #region Start and stop.

    /// <summary>
    /// Starts the local server or client using configured settings.
    /// </summary>
    /// <param name="server">True to start server.</param>
    public abstract bool StartConnection(bool server);


    /// <summary>
    /// Stops the local server or client.
    /// </summary>
    /// <param name="server">True to stop server.</param>
    public abstract bool StopConnection(bool server);


    /// <summary>
    /// Stops a remote client from the server, disconnecting the client.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    /// <param name="immediate">True to disconnect immediately.</param>
    public abstract bool StopConnection(int connectionId, bool immediate);


    /// <summary>
    /// Stops both client and server.
    /// </summary>
    public abstract void Shutdown();

    #endregion

    #region Channels.

    /// <summary>
    /// Gets the MTU for a channel.
    /// </summary>
    /// <param name="channel">Channel to get MTU for.</param>
    /// <returns>MTU of channel.</returns>
    public abstract int GetMTU(byte channel);

    #endregion


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO release managed resources here
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    public virtual void Initialize(NetworkManager networkManager)
    {
        NetworkManager = networkManager;
    }
}