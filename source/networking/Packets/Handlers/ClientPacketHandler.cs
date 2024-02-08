using Korpi.Networking.Connections;
using Korpi.Networking.Transports;

namespace Korpi.Networking.Packets.Handlers;

/// <summary>
/// Handles packets received on server, from clients.
/// </summary>
internal class ClientPacketHandler<T> : PacketHandlerCollection
{
    private readonly List<Action<NetworkConnection, T, Channel>> _handlers = new();
    public override bool RequireAuthentication { get; }


    public ClientPacketHandler(bool requireAuthentication)
    {
        RequireAuthentication = requireAuthentication;
    }

    public override void RegisterHandler(object obj)
    {
        if (obj is Action<NetworkConnection, T, Channel> handler)
            _handlers.Add(handler);
    }

    public override void UnregisterHandler(object obj)
    {
        if (obj is Action<NetworkConnection, T, Channel> handler)
            _handlers.Remove(handler);
    }

    public override void InvokeHandlers(NetworkConnection conn, IPacket packet, Channel channel)
    {
        if (packet is not T tPacket)
            return;
        
        foreach (Action<NetworkConnection, T, Channel> handler in _handlers)
            handler.Invoke(conn, tPacket, channel);
    }
}