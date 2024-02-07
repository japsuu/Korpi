using Korpi.Networking.Packets;

namespace Korpi.Networking.ConnectionState;

/// <summary>
/// Container about data received on the server.
/// </summary>
public struct ServerReceivedDataArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public INetworkPacket Data;

    /// <summary>
    /// ConnectionId from which client sent data, if data was received on the server.
    /// </summary>
    public int ConnectionId;


    public ServerReceivedDataArgs(INetworkPacket data, int connectionId)
    {
        Data = data;
        ConnectionId = connectionId;
    }
}