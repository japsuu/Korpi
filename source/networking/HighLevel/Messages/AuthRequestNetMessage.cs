namespace Korpi.Networking.HighLevel.Messages;

public struct AuthRequestNetMessage : NetMessage
{
    public byte AuthenticationMethod { get; set; }


    public AuthRequestNetMessage(byte authenticationMethod)
    {
        AuthenticationMethod = authenticationMethod;
    }
}