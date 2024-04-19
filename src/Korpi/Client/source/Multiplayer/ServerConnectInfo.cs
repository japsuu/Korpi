namespace Korpi.Client.Multiplayer;

/// <summary>
/// Contains the necessary information to connect to a server.
/// </summary>
public readonly struct ServerConnectInfo
{
    public readonly string Address;
    public readonly ushort Port;
    
    
    public ServerConnectInfo(string address, ushort port)
    {
        Address = address;
        Port = port;
    }


    public override string ToString() => $"{Address}:{Port}";
}