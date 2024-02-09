using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;

namespace Korpi.Networking.Transports.Singleplayer;

public class SingleplayerTransport : Transport
{
    public override event Action<ClientConnectionStateArgs>? LocalClientConnectionStateChanged;
    public override event Action<ServerConnectionStateArgs>? LocalServerConnectionStateChanged;
    public override event Action<RemoteConnectionStateArgs>? RemoteClientConnectionStateChanged;

    public override void HandleLocalClientConnectionStateChange(ClientConnectionStateArgs connectionStateArgs)
    {
        throw new NotImplementedException();
    }


    public override void HandleLocalServerConnectionStateChange(ServerConnectionStateArgs connectionStateArgs)
    {
        throw new NotImplementedException();
    }


    public override void HandleRemoteClientConnectionStateChange(RemoteConnectionStateArgs connectionStateArgs)
    {
        throw new NotImplementedException();
    }


    public override LocalConnectionState GetLocalConnectionState(bool asServer) => throw new NotImplementedException();


    public override RemoteConnectionState GetRemoteConnectionState(int connectionId) => throw new NotImplementedException();


    public override string GetRemoteConnectionAddress(int connectionId) => throw new NotImplementedException();


    public override void SendToServer(Channel channel, IPacket packet)
    {
        throw new NotImplementedException();
    }


    public override void SendToClient(Channel channel, IPacket packet, int connectionId)
    {
        throw new NotImplementedException();
    }


    public override event Action<ClientReceivedPacketArgs>? LocalClientReceivedPacket;
    public override event Action<ServerReceivedPacketArgs>? LocalServerReceivedPacket;

    public override void HandleLocalClientReceivedPacket(ClientReceivedPacketArgs receivedDataArgs)
    {
        throw new NotImplementedException();
    }


    public override void HandleLocalServerReceivedPacket(ServerReceivedPacketArgs receivedPacketArgs)
    {
        throw new NotImplementedException();
    }


    public override void IterateIncoming(bool asServer)
    {
        throw new NotImplementedException();
    }


    public override void IterateOutgoing(bool asServer)
    {
        throw new NotImplementedException();
    }


    public override bool StartLocalConnection(bool server) => throw new NotImplementedException();


    public override bool StopLocalConnection(bool server) => throw new NotImplementedException();


    public override bool StopRemoteConnection(int connectionId, bool immediate) => throw new NotImplementedException();


    public override void Shutdown()
    {
        throw new NotImplementedException();
    }
}