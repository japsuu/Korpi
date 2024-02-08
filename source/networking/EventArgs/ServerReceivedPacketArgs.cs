using Korpi.Networking.Packets;
using Korpi.Networking.Transports;

namespace Korpi.Networking.EventArgs;

/// <summary>
/// Container about data received on the server.
/// </summary>
public struct ServerReceivedPacketArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public IPacket Packet;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public Channel Channel;

    /// <summary>
    /// Connection from which client sent data, if data was received on the server.
    /// </summary>
    public int ConnectionId;


    public ServerReceivedPacketArgs(IPacket packet, Channel channel, int connectionId)
    {
        Packet = packet;
        Channel = channel;
        ConnectionId = connectionId;
    }
}