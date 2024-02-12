using Korpi.Networking.HighLevel.Messages;

namespace Korpi.Networking.Utility;

internal static class PacketHelper
{
    /// <summary>
    /// Gets the key for a message type.
    /// </summary>
    internal static ushort GetKey<T>() where T : struct, NetMessage
    {
        return (typeof(T).FullName ?? throw new InvalidOperationException()).GetStableHashU16();
    }
    
    /// <summary>
    /// Gets the key for a message type.
    /// </summary>
    internal static ushort GetKey(this NetMessage type)
    {
        return (type.GetType().FullName ?? throw new InvalidOperationException()).GetStableHashU16();
    }
}