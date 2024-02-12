namespace Korpi.Networking.HighLevel.Messages;

/// <summary>
/// Sent by the client to the server to authenticate with a password.
/// </summary>
public struct AuthPasswordNetMessage : NetMessage
{
    public string Username { get; set; }
    public string Password { get; set; }


    public AuthPasswordNetMessage(string username, string password)
    {
        Username = username;
        Password = password;
    }
}