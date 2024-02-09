using Common.Logging;
using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using Korpi.Networking.Packets;
using Korpi.Networking.Transports;

namespace Korpi.Networking.Authenticating;

public class PasswordAuthenticator : Authenticator
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(PasswordAuthenticator));
    
    private readonly string _password;
    
    public override event Action<NetworkConnection, bool>? ConcludedAuthenticationResult;


    public PasswordAuthenticator(string password)
    {
        _password = password;
    }


    public override void Initialize(NetworkManager networkManager)
    {
        base.Initialize(networkManager);
        
        // Server listen for packets from client.
        NetworkManager.Server.RegisterPacketHandler<AuthPasswordPacket>(OnReceiveAuthPasswordPacket, false);

        // Client listen to packets from server.
        NetworkManager.Client.RegisterPacketHandler<AuthResponsePacket>(OnReceiveAuthResponsePacket);
        NetworkManager.Client.RegisterPacketHandler<AuthRequestPacket>(OnReceiveAuthRequestPacket);
    }


    private void OnReceiveAuthRequestPacket(AuthRequestPacket packet, Channel channel)
    {
        Logger.Info("Received authentication request from server.");
        
        if (!NetworkManager.Client.Started)
        {
            Logger.Error("Client not started. Cannot authenticate.");
            return;
        }

        if (packet.AuthenticationMethod != 0)
        {
            Logger.Error("Server requested an unsupported authentication method.");
            return;
        }
        // Respond to the server with the password.
        AuthPasswordPacket pb = new("tester", _password);
        NetworkManager.Client.SendPacketToServer(pb);
    }


    public override void OnRemoteConnection(NetworkConnection connection)
    {
        base.OnRemoteConnection(connection);
        
        // Send the client a authentication request.
        AuthRequestPacket packet = new AuthRequestPacket(0);
        NetworkManager.Server.SendPacketToClient(connection, packet, false);
    }


    private void OnReceiveAuthPasswordPacket(NetworkConnection conn, AuthPasswordPacket packet, Channel channel)
    {
        /* If client is already authenticated this could be an attack. Connections
         * are removed when a client disconnects so there is no reason they should
         * already be considered authenticated. */
        if (conn.IsAuthenticated)
        {
            conn.Disconnect(true);
            return;
        }

        bool validName = !string.IsNullOrWhiteSpace(packet.Username);
        bool correctPassword = packet.Password == _password;
        bool isAuthSuccess = validName && correctPassword;
        
        string? error = null;
        if (!validName)
            error = "Invalid username.";
        else if (!correctPassword)
            error = "Invalid password.";
        
        AuthResponsePacket response = new AuthResponsePacket(isAuthSuccess, error);
        NetworkManager.Server.SendPacketToClient(conn, response, false);
        
        /* Invoke result. This is handled internally to complete the connection or kick client.
         * It's important to call this after sending the broadcast so that the broadcast
         * makes it out to the client before the kick. */
        ConcludedAuthenticationResult?.Invoke(conn, correctPassword);
    }


    private void OnReceiveAuthResponsePacket(AuthResponsePacket packet, Channel channel)
    {
        string message = packet.Success ? "Authenticated." : "Authentication failed.";
        if (!string.IsNullOrWhiteSpace(packet.Reason))
            message += $" Reason: {packet.Reason}";
        Logger.Info(message);
    }
}