using Korpi.Networking.Transports;

namespace Korpi.Networking.EventArgs;

/// <summary>
/// Container about data received on the server.
/// </summary>
public readonly struct ServerReceivedDataArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public readonly ArraySegment<byte> Segment;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;

    /// <summary>
    /// Connection of client which sent the data.
    /// </summary>
    public readonly int ConnectionId;


    public ServerReceivedDataArgs(ArraySegment<byte> segment, Channel channel, int connectionId)
    {
        Segment = segment;
        Channel = channel;
        ConnectionId = connectionId;
    }
}