using Korpi.Networking.Connections;
using Korpi.Networking.Transports;

namespace Korpi.Networking.Packets.Handlers;

internal abstract class PacketHandlerCollection
{
    public abstract void RegisterHandler(object obj);
    public abstract void UnregisterHandler(object obj);
    public abstract void InvokeHandlers(IPacket packet, Channel channel);
    public abstract void InvokeHandlers(NetworkConnection conn, IPacket packet, Channel channel);
    public abstract bool RequireAuthentication { get; }
}