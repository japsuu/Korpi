namespace Korpi.Networking.EventArgs;

public struct ServerConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public LocalConnectionState ConnectionState;


    public ServerConnectionStateArgs(LocalConnectionState connectionState)
    {
        ConnectionState = connectionState;
    }
}