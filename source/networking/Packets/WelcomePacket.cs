namespace Korpi.Networking.Packets;

public struct WelcomePacket : IPacket
{
    public ushort ClientId { get; set; }


    public WelcomePacket(ushort clientId)
    {
        ClientId = clientId;
    }
}