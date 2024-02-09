using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;

namespace Korpi.Networking.Transports.Singleplayer;

public class SingleplayerTransport : Transport
{
    public override event Action<ClientConnectionStateArgs>? LocalClientConnectionStateChanged;
    public override event Action<ServerConnectionStateArgs>? LocalServerConnectionStateChanged;
    public override event Action<RemoteConnectionStateArgs>? RemoteClientConnectionStateChanged;
    public override event Action<ClientReceivedPacketArgs>? LocalClientReceivedPacket;
    public override event Action<ServerReceivedPacketArgs>? LocalServerReceivedPacket;

    private readonly Queue<(Channel channel, IPacket packet)> _incomingPacketsClient = new();
    private readonly Queue<(Channel channel, IPacket packet)> _outgoingPacketsClient = new();
    private readonly Queue<(Channel channel, IPacket packet)> _incomingPacketsServer = new();
    private readonly Queue<(Channel channel, IPacket packet)> _outgoingPacketsServer = new();


    public override LocalConnectionState GetLocalConnectionState(bool asServer)
    {
        throw new NotImplementedException();
    }


    public override RemoteConnectionState GetRemoteConnectionState(int connectionId)
    {
        throw new NotImplementedException();
    }


    public override string GetRemoteConnectionAddress(int connectionId) => "none";


    public override void SendToServer(Channel channel, IPacket packet)
    {
        Console.WriteLine($"Sending packet to server: {packet.GetType().Name}");
        _outgoingPacketsClient.Enqueue((channel, packet));
    }


    public override void SendToClient(Channel channel, IPacket packet, int connectionId)
    {
        Console.WriteLine($"Sending packet to client: {packet.GetType().Name}");
        _outgoingPacketsServer.Enqueue((channel, packet));
    }


    public override void IterateIncoming(bool asServer)
    {
        if (asServer)
            while (_incomingPacketsServer.Count > 0)
            {
                var (channel, packet) = _incomingPacketsServer.Dequeue();
                LocalServerReceivedPacket?.Invoke(new ServerReceivedPacketArgs(packet, channel, 0));
            }
        else
            while (_incomingPacketsClient.Count > 0)
            {
                var (channel, packet) = _incomingPacketsClient.Dequeue();
                LocalClientReceivedPacket?.Invoke(new ClientReceivedPacketArgs(packet, channel));
            }
    }


    public override void IterateOutgoing(bool asServer)
    {
        if (asServer)
            while (_outgoingPacketsServer.Count > 0)
            {
                var (channel, packet) = _outgoingPacketsServer.Dequeue();
                _incomingPacketsClient.Enqueue((channel, packet));
            }
        else
            while (_outgoingPacketsClient.Count > 0)
            {
                var (channel, packet) = _outgoingPacketsClient.Dequeue();
                _incomingPacketsServer.Enqueue((channel, packet));
            }
    }


    public override void StartLocalConnection(bool server)
    {
        if (server)
        {
            
            Task.Run(
                () =>
                {
                    Task.Delay(500).Wait();
                    
                    LocalServerConnectionStateChanged?.Invoke(new ServerConnectionStateArgs(LocalConnectionState.Starting));
                    
                    Task.Delay(500).Wait();
                    
                    LocalServerConnectionStateChanged?.Invoke(new ServerConnectionStateArgs(LocalConnectionState.Started));
                }
            );
        }
        else
        {
            Task.Run(
                () =>
                {
                    Task.Delay(550).Wait();
                    
                    LocalClientConnectionStateChanged?.Invoke(new ClientConnectionStateArgs(LocalConnectionState.Starting));
                    
                    Task.Delay(550).Wait();

                    LocalClientConnectionStateChanged?.Invoke(new ClientConnectionStateArgs(LocalConnectionState.Started));
                    
                    Task.Delay(550).Wait();
                    
                    RemoteClientConnectionStateChanged?.Invoke(new RemoteConnectionStateArgs(RemoteConnectionState.Started, 0));
                }
            );
        }
    }


    public override void StopLocalConnection(bool server)
    {
        if (server)
        {
            Task.Run(
                () =>
                {
                    Task.Delay(500).Wait();
                    
                    LocalServerConnectionStateChanged?.Invoke(new ServerConnectionStateArgs(LocalConnectionState.Stopping));
                    
                    Task.Delay(500).Wait();
                    
                    LocalServerConnectionStateChanged?.Invoke(new ServerConnectionStateArgs(LocalConnectionState.Stopped));
                }
            );
        }
        else
        {
            Task.Run(
                () =>
                {
                    Task.Delay(550).Wait();
                    
                    LocalClientConnectionStateChanged?.Invoke(new ClientConnectionStateArgs(LocalConnectionState.Stopping));
                    
                    Task.Delay(550).Wait();
                    
                    LocalClientConnectionStateChanged?.Invoke(new ClientConnectionStateArgs(LocalConnectionState.Stopped));
                    
                    Task.Delay(550).Wait();
                    
                    RemoteClientConnectionStateChanged?.Invoke(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, 0));
                }
            );
        }
    }


    public override void StopRemoteConnection(int connectionId, bool immediate)
    {
        throw new NotImplementedException();
    }


    public override void Shutdown()
    {
    }


    public override float GetTimeout(bool asServer) => 0;


    public override void SetTimeout(float value, bool asServer)
    {
        
    }


    public override int GetMaximumClients() => 0;


    public override void SetMaximumClients(int value)
    {
        
    }


    public override void SetClientAddress(string address)
    {
        
    }


    public override string GetClientAddress() => "none";


    public override void SetServerBindAddress(string address)
    {
        
    }


    public override string GetServerBindAddress() => "none";


    public override void SetPort(ushort port)
    {
        
    }


    public override ushort GetPort() => 0;
}