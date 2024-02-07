namespace Korpi.Networking;

public static class NetworkManager
{
    private static NetworkTransport transport = null!;

    public static NetServerManager Server { get; private set; } = null!;
    public static NetClientManager Client { get; private set; } = null!;


    public static void Initialize(NetworkTransport transportLayer)
    {
        transport = transportLayer;
        Server = new NetServerManager(transport);
        Client = new NetClientManager(transport);
    }
}