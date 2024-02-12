namespace Korpi.Networking.HighLevel.Messages;

/// <summary>
/// Packet sent to all clients when a client connects or disconnects.
/// </summary>
public struct ClientConnectionChangeNetMessage : NetMessage
{
    public int ClientId { get; set; }
    public bool Connected { get; set; }


    public ClientConnectionChangeNetMessage(int clientId, bool connected)
    {
        ClientId = clientId;
        Connected = connected;
    }
}