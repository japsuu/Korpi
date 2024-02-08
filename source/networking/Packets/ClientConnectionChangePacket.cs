namespace Korpi.Networking.Packets;

/// <summary>
/// Packet sent to all clients when a client connects or disconnects.
/// </summary>
public struct ClientConnectionChangePacket : IPacket
{
    public int ClientId { get; set; }
    public bool Connected { get; set; }


    public ClientConnectionChangePacket(int clientId, bool connected)
    {
        ClientId = clientId;
        Connected = connected;
    }
}