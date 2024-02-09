namespace Korpi.Networking.EventArgs;

public readonly struct ConnectedClientsListArgs
{
    public readonly List<int> ClientIds;
    
    
    public ConnectedClientsListArgs(List<int> clientIds)
    {
        ClientIds = clientIds;
    }
}