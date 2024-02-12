﻿using Common.Logging;
using Korpi.Networking.Connections;
using Korpi.Networking.Transports;

namespace Korpi.Networking;

public class NetworkManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetworkManager));
    
    public readonly TransportManager TransportManager;
    public readonly NetServerManager Server;
    public readonly NetClientManager Client;


    public event Action? Update;


    public NetworkManager(Transport transportLayer)
    {
        transportLayer.Initialize(this);
        TransportManager = new TransportManager(this, transportLayer);
        Server = new NetServerManager(this, TransportManager);
        Client = new NetClientManager(this, TransportManager);
    }
    
    
    public void Tick()
    {
        Update?.Invoke();
        IteratePackets(true);
        IteratePackets(false);
    }


    public static void ClearClientsCollection(Dictionary<int,NetworkConnection> clients)
    {
        // Dispose of all clients and clear the collection.
        foreach (NetworkConnection client in clients.Values)
        {
            client.Dispose();
        }
        clients.Clear();
    }


    /// <summary>
    /// Iterates incoming or outgoing packets.
    /// </summary>
    /// <param name="incoming">True to iterate incoming.</param>
    private void IteratePackets(bool incoming)
    {
        if (incoming)
        {
            TransportManager.IterateIncoming(true);
            TransportManager.IterateIncoming(false);
        }
        else
        {
            TransportManager.IterateOutgoing(true);
            TransportManager.IterateOutgoing(false);
        }
    }
}