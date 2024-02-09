using Korpi.Networking.Connections;

namespace Korpi.Networking.EventArgs;

public readonly struct ClientConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public readonly LocalConnectionState ConnectionState;


    public ClientConnectionStateArgs(LocalConnectionState connectionState)
    {
        ConnectionState = connectionState;
    }
}