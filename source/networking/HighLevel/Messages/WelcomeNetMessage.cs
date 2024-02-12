namespace Korpi.Networking.HighLevel.Messages;

public struct WelcomeNetMessage : NetMessage
{
    public ushort ClientId { get; set; }


    public WelcomeNetMessage(ushort clientId)
    {
        ClientId = clientId;
    }
}