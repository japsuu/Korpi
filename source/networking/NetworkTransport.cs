using Korpi.Networking.ConnectionState;
using Korpi.Networking.Packets;

namespace Korpi.Networking;

/// <summary>
/// Represents an object that can handle network messages.
/// </summary>
public abstract class NetworkTransport
{
    public NetworkTransport()
    {
    }


    #region Settings

    public abstract string ServerBindAddress { get; set; }
    public abstract string ClientConnectAddress { get; set; }
    public abstract ushort ServerBindPort { get; set; }
    public abstract ushort ClientConnectPort { get; set; }
    public abstract int MaxConnections { get; set; }

    #endregion

    #region Connection State Changes

    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    public abstract event Action<ClientConnectionStateArgs> ClientConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for the local server.
    /// </summary>
    public abstract event Action<ServerConnectionStateArgs> ServerConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    public abstract event Action<RemoteConnectionStateArgs> RemoteConnectionStateChanged;

    #endregion


    #region Sending Data

    /// <summary>
    /// Send data to the server.
    /// </summary>
    /// <param name="packet">Data to send.</param>
    public abstract void ClientSendToServer(INetworkPacket packet);


    /// <summary>
    /// Send data to a client.
    /// </summary>
    /// <param name="packet">Data to send.</param>
    /// <param name="connectionId">ConnectionId to send the packet to.</param>
    public abstract void ServerSendToClient(INetworkPacket packet, int connectionId);

    #endregion


    #region Receiving Data

    /// <summary>
    /// Called when the client receives data.
    /// </summary>
    public abstract event Action<ClientReceivedDataArgs> OnClientReceivedData;

    /// <summary>
    /// Called when the server receives data.
    /// </summary>
    public abstract event Action<ServerReceivedDataArgs> OnServerReceivedData;

    #endregion

    #region Iterating Data

    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="isServer">True to process data received on the server.</param>
    public abstract void IterateIncomingData(bool isServer);


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="isServer">True to process data received on the server.</param>
    public abstract void IterateOutgoingData(bool isServer);

    #endregion

    #region Starting and Stopping Connections

    /// <summary>
    /// Starts the server using configured settings.
    /// </summary>
    public abstract bool StartServer();


    /// <summary>
    /// Starts the local client using configured settings.
    /// </summary>
    public abstract bool StartClient();


    /// <summary>
    /// Stops the server.
    /// </summary>
    public abstract bool StopServer();


    /// <summary>
    /// Stops the local client.
    /// </summary>
    public abstract bool StopClient();


    /// <summary>
    /// Stops a remote client from the server, disconnecting the client.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    /// <param name="immediately">True to abruptly stop the client socket. The technique used to accomplish immediate disconnects may vary depending on the transport.</param>
    public abstract bool StopConnection(int connectionId, bool immediately);

    #endregion
}