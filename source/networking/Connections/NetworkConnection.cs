using Common.Logging;
using Korpi.Networking.Transports;

namespace Korpi.Networking.Connections;

/// <summary>
/// A container for a connected client used to perform actions on and gather information for the declared client.
/// </summary>
public class NetworkConnection : IDisposable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetworkConnection));

    private readonly NetServerManager _serverManager;
    private readonly Transport _transport;

    /// <summary>
    /// True if this connection is authenticated. Only available to server.
    /// </summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>
    /// True if this connection is valid and not Disconnecting.
    /// </summary>
    public bool IsActive => IsValid && !Disconnecting;

    /// <summary>
    /// True if this connection is valid. An invalid connection indicates no client is set for this reference.
    /// </summary>
    public bool IsValid => ClientId >= 0;

    /// <summary>
    /// Unique Id for this connection.
    /// </summary>
    public readonly int ClientId;

    /// <summary>
    /// True if this connection is being disconnected. Only available to server.
    /// </summary>
    public bool Disconnecting { get; private set; }

    
    public NetworkConnection(NetServerManager serverManager, Transport transport, int clientId, bool asServer)
    {
        _serverManager = serverManager;
        _transport = transport;
        ClientId = clientId;

        if (asServer)
        {
            
        }
    }


    /// <summary>
    /// Disconnects this connection. Only available on the server.
    /// </summary>
    /// <param name="immediate">True to disconnect immediately.</param>
    public void Disconnect(bool immediate)
    {
        if (!IsValid)
        {
            Logger.Warn("Disconnect called on an invalid connection.");
            return;
        }

        if (Disconnecting)
        {
            Logger.Warn($"ClientId {ClientId} is already disconnecting.");
            return;
        }

        SetDisconnecting(true);

        _transport.StopConnection(ClientId, immediate);
    }


    /// <summary>
    /// Sets connection as authenticated.
    /// </summary>
    internal void SetAuthenticated()
    {
        IsAuthenticated = true;
    }


    /// <summary>
    /// Sets Disconnecting boolean for this connection.
    /// </summary>
    internal void SetDisconnecting(bool value)
    {
        Disconnecting = value;
    }


    public override string ToString()
    {
        string ip = _transport.GetConnectionAddress(ClientId);
        return $"Id [{ClientId}] Address [{ip}]";
    }


    public void Dispose()
    {
        // TODO release managed resources here
    }
}