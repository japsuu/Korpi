using Korpi.Networking.Connections;

namespace Korpi.Networking.Authenticating;

public abstract class Authenticator
{
    protected readonly NetworkManager NetworkManager;


    public Authenticator(NetworkManager networkManager)
    {
        NetworkManager = networkManager;
    }


    /// <summary>
    /// Called when authenticator has concluded a result for a connection. Boolean is true if authentication passed, false if failed.
    /// Server listens for this event automatically.
    /// </summary>
    public abstract event Action<NetworkConnection, bool> ConcludedAuthenticationResult;


    /// <summary>
    /// Called on the server immediately after a client connects. Can be used to send data to the client for authentication.
    /// </summary>
    /// <param name="connection">Connection which is not yet authenticated.</param>
    public virtual void OnRemoteConnection(NetworkConnection connection)
    {
    }
}