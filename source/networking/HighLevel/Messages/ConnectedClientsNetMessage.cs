namespace Korpi.Networking.HighLevel.Messages;

/// <summary>
/// Packet sent to a new client when they connect to the server.
/// </summary>
public struct ConnectedClientsNetMessage : NetMessage
{
    public List<int>? ClientIds { get; set; }
    
    
    public ConnectedClientsNetMessage(List<int>? clientIds)
    {
        ClientIds = clientIds;
    }
}