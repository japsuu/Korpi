using Korpi.Networking;
using Korpi.Networking.Authenticating;

namespace Korpi.Server;

public struct GameServerConfiguration
{
    public string BindAddress { get; set; }
    public ushort BindPort { get; set; }
    public int MaxConnections { get; set; }
    public Authenticator? Authenticator { get; set; }


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
    public static GameServerConfiguration Default()
    {
        return new GameServerConfiguration("127.0.0.1", 7531, 64, null);
    }
    
    
    public GameServerConfiguration WithPasswordAuthentication(string password, NetworkManager networkManager)
    {
        Authenticator = new PasswordAuthenticator(networkManager, password);
        return this;
    }
}