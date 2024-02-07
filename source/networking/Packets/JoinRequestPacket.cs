namespace Korpi.Networking.Packets;

/// <summary>
/// Packet sent from the client to the server when requesting to join the game.
/// </summary>
public struct JoinRequestPacket : INetworkPacket
{
    public string Username { get; set; }
    public string Password { get; set; }
    
    public JoinRequestPacket(string username, string password)
    {
        Username = username;
        Password = password;
    }
}