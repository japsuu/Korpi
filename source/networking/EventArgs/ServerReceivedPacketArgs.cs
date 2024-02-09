using Korpi.Networking.Packets;
using Korpi.Networking.Transports;

namespace Korpi.Networking.EventArgs;

/// <summary>
/// Container about data received on the server.
/// </summary>
public readonly struct ServerReceivedPacketArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public readonly IPacket Packet;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;

    /// <summary>
    /// Connection from which client sent data, if data was received on the server.
    /// </summary>
    public readonly int ConnectionId;


    public ServerReceivedPacketArgs(IPacket packet, Channel channel, int connectionId)
    {
        Packet = packet;
        Channel = channel;
        ConnectionId = connectionId;
    }
}