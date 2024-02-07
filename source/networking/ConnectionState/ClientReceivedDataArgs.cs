using Korpi.Networking.Packets;

namespace Korpi.Networking.ConnectionState;

/// <summary>
/// Container about data received on the local client.
/// </summary>
public struct ClientReceivedDataArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public INetworkPacket Data;


    public ClientReceivedDataArgs(INetworkPacket data)
    {
        Data = data;
    }
}