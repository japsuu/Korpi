using Korpi.Networking.HighLevel.Authentication;

namespace Korpi.Server;

public struct GameServerConfiguration
{
    public string BindAddress;
    public ushort BindPort;
    public int MaxConnections;
    public Authenticator? Authenticator;


    public GameServerConfiguration(string bindAddress, ushort bindPort, int maxConnections, Authenticator? authenticator)
    {
        BindAddress = bindAddress;
        BindPort = bindPort;
        MaxConnections = maxConnections;
        Authenticator = authenticator;
    }


    /// <summary>
    /// Default configuration for a localhost game server. Bind address is "127.0.0.1" and port is 7531. Max connections is 64.
    /// </summary>
    /// <returns></returns>
    public static GameServerConfiguration Localhost()
    {
        return new GameServerConfiguration("127.0.0.1", 7531, 64, null);
    }
    
    
    public GameServerConfiguration WithPasswordAuthentication(string password)
    {
        Authenticator = new PasswordAuthenticator(password);
        return this;
    }
}