using Korpi.Networking.Packets;
using Korpi.Networking.Transports;

namespace Korpi.Networking.EventArgs;

/// <summary>
/// Container about data received on the local client.
/// </summary>
public readonly struct ClientReceivedPacketArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public readonly IPacket Packet;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;


    public ClientReceivedPacketArgs(IPacket packet, Channel channel)
    {
        Packet = packet;
        Channel = channel;
    }
}