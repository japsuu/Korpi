namespace Korpi.Networking.Transports.LiteNetLib.Sockets.Server;

internal struct RemoteConnectionEvent
{
    public readonly bool Connected;
    public readonly int ConnectionId;


    public RemoteConnectionEvent(bool connected, int connectionId)
    {
        Connected = connected;
        ConnectionId = connectionId;
    }
}