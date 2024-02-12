using Korpi.Networking.LowLevel.NetStack.Serialization;

namespace Korpi.Networking.HighLevel.Messages;

/// <summary>
/// Represents an message (packet) that can be sent over the network.
/// You can cache instances of this class, as it is immutable and won't be reused.
/// </summary>
public abstract class NetMessage
{
    protected NetMessage()
    {
    }
    
    
    /// <summary>
    /// Serializes this message into a bit buffer.
    /// The message ID is automatically added to the buffer.
    /// </summary>
    /// <param name="buffer">Empty buffer to serialize the message into.</param>
    public void Serialize(BitBuffer buffer)
    {
        ushort id = MessageManager.MessageIdCache.GetId(GetType());
        buffer.AddUShort(id);
        SerializeInternal(buffer);
    }

    
    /// <summary>
    /// Deserializes this message from a bit buffer.
    /// The message ID is automatically read from the buffer.
    /// </summary>
    /// <param name="buffer">Buffer containing the serialized message.</param>
    public void Deserialize(BitBuffer buffer)
    {
        DeserializeInternal(buffer);
    }


    protected abstract void SerializeInternal(BitBuffer buffer);
    protected abstract void DeserializeInternal(BitBuffer buffer);
}