namespace Korpi.Networking.HighLevel.Messages;

/// <summary>
/// Sent by the server to the client to indicate the result of the authentication process.
/// </summary>
public struct AuthResponseNetMessage : NetMessage
{
    public bool Success { get; set; }
    public string? Reason { get; set; }


    public AuthResponseNetMessage(bool success, string? reason)
    {
        Success = success;
        Reason = reason;
    }
}