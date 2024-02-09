namespace Korpi.Networking.Packets;

public struct AuthRequestPacket : IPacket
{
    public byte AuthenticationMethod { get; set; }


    public AuthRequestPacket(byte authenticationMethod)
    {
        AuthenticationMethod = authenticationMethod;
    }
}