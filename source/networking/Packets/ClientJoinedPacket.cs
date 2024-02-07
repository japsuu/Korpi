namespace Korpi.Networking.Packets;

/// <summary>
/// Packet sent from the server to all clients when a new client joins.
/// </summary>
public struct ClientJoinedPacket : INetworkPacket
{
    public ushort PlayerId { get; set; }
    public string Username { get; set; }
    
    public ClientJoinedPacket(ushort playerId, string username)
    {
        PlayerId = playerId;
        Username = username;
    }
}