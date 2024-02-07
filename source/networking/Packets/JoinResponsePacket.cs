namespace Korpi.Networking.Packets;

/// <summary>
/// Packet sent from the server to the client when responding to a join request.
/// </summary>
public struct JoinResponsePacket : INetworkPacket
{
    public ushort PlayerId { get; set; }
    public bool Success { get; set; }
    public string? Reason { get; set; }


    public JoinResponsePacket(ushort playerId, bool success, string? reason)
    {
        PlayerId = playerId;
        Success = success;
        Reason = reason;
    }
}