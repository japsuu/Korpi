namespace Korpi.Networking.EventArgs;

public struct ClientConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public LocalConnectionState ConnectionState;


    public ClientConnectionStateArgs(LocalConnectionState connectionState)
    {
        ConnectionState = connectionState;
    }
}