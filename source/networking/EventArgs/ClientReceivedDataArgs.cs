using Korpi.Networking.Packets;
using Korpi.Networking.Transports;

namespace Korpi.Networking.EventArgs;

/// <summary>
/// Container about data received on the local client.
/// </summary>
public struct ClientReceivedDataArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public IPacket Data;
    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public Channel Channel;

    public ClientReceivedDataArgs(IPacket data, Channel channel)
    {
        Data = data;
        Channel = channel;
    }
}