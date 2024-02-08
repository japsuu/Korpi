namespace Korpi.Networking.Packets;

/// <summary>
/// Sent by the server to the client to indicate the result of the authentication process.
/// </summary>
public struct AuthResponsePacket : IPacket
{
    public bool Success { get; set; }
    public string? Reason { get; set; }


    public AuthResponsePacket(bool success, string? reason)
    {
        Success = success;
        Reason = reason;
    }
}