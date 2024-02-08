namespace Korpi.Networking.Packets;

/// <summary>
/// Packet sent to a new client when they connect to the server.
/// </summary>
public struct ConnectedClientsPacket : IPacket
{
    public List<int> ClientIds { get; set; }
    
    
    public ConnectedClientsPacket(List<int> clientIds)
    {
        ClientIds = clientIds;
    }
}