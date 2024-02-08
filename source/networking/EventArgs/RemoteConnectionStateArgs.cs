namespace Korpi.Networking.EventArgs;

public struct RemoteConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public RemoteConnectionState ConnectionState;
    
    /// <summary>
    /// ConnectionId for which client the state changed. Will be 0 if <see cref="ConnectionState"/> was for the server.
    /// </summary>
    public int ConnectionId;


    public RemoteConnectionStateArgs(RemoteConnectionState state, int connectionEventConnectionId)
    {
        ConnectionState = state;
        ConnectionId = connectionEventConnectionId;
    }
}