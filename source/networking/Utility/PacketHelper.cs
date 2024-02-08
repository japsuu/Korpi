using Korpi.Networking.Packets;

namespace Korpi.Networking.Utility;

internal static class PacketHelper
{
    /// <summary>
    /// Gets the key for a packet type.
    /// </summary>
    internal static ushort GetKey<T>() where T : struct, IPacket
    {
        return (typeof(T).FullName ?? throw new InvalidOperationException()).GetStableHashU16();
    }
    
    /// <summary>
    /// Gets the key for a packet type.
    /// </summary>
    internal static ushort GetKey(this IPacket type)
    {
        return (type.GetType().FullName ?? throw new InvalidOperationException()).GetStableHashU16();
    }
}