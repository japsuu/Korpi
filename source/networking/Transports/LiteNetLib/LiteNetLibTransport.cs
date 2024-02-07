using Korpi.Networking.ConnectionState;
using Korpi.Networking.Packets;

namespace Korpi.Networking.Transports.LiteNetLib;

public class LiteNetLibTransport : NetworkTransport
{
    public override string ServerBindAddress { get; set; } = string.Empty;
    public override string ClientConnectAddress { get; set; } = string.Empty;
    public override ushort ServerBindPort { get; set; }
    public override ushort ClientConnectPort { get; set; }
    public override int MaxConnections { get; set; }
    public override event Action<ClientConnectionStateArgs> ClientConnectionStateChanged;
    public override event Action<ServerConnectionStateArgs> ServerConnectionStateChanged;
    public override event Action<RemoteConnectionStateArgs> RemoteConnectionStateChanged;

    public override void ClientSendToServer(INetworkPacket packet)
    {
        throw new NotImplementedException();
    }


    public override void ServerSendToClient(INetworkPacket packet, int connectionId)
    {
        throw new NotImplementedException();
    }


    public override event Action<ClientReceivedDataArgs> OnClientReceivedData;
    public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

    public override void IterateIncomingData(bool isServer)
    {
        throw new NotImplementedException();
    }


    public override void IterateOutgoingData(bool isServer)
    {
        throw new NotImplementedException();
    }


    public override bool StartServer()
    {
        throw new NotImplementedException();
    }


    public override bool StartClient()
    {
        throw new NotImplementedException();
    }


    public override bool StopServer()
    {
        throw new NotImplementedException();
    }


    public override bool StopClient()
    {
        throw new NotImplementedException();
    }


    public override bool StopConnection(int connectionId, bool immediately)
    {
        throw new NotImplementedException();
    }
}