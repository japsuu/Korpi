namespace Korpi.Networking.Packets;

/// <summary>
/// Sent by the client to the server to authenticate with a password.
/// </summary>
public struct AuthPasswordPacket : IPacket
{
    public string Username { get; set; }
    public string Password { get; set; }


    public AuthPasswordPacket(string username, string password)
    {
        Username = username;
        Password = password;
    }
}