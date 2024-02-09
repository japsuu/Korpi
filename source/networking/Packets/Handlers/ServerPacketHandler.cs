using Korpi.Networking.Connections;
using Korpi.Networking.Transports;

namespace Korpi.Networking.Packets.Handlers;

/// <summary>
/// Handles packets received on clients, from the server.
/// </summary>
internal class ServerPacketHandler<T> : PacketHandlerCollection
{
    private readonly List<Action<T, Channel>> _handlers = new();

    public override bool RequireAuthentication => false;


    public override void RegisterHandler(object obj)
    {
        if (obj is Action<T, Channel> handler)
            _handlers.Add(handler);
    }


    public override void UnregisterHandler(object obj)
    {
        if (obj is Action<T, Channel> handler)
            _handlers.Remove(handler);
    }


    public override void InvokeHandlers(IPacket packet, Channel channel)
    {
        if (packet is not T tPacket)
            return;

        foreach (Action<T, Channel> handler in _handlers)
            handler.Invoke(tPacket, channel);
    }


    public override void InvokeHandlers(NetworkConnection conn, IPacket packet, Channel channel)
    {
        // Client does not handle packets from other clients.
        throw new NotImplementedException();
    }
}