namespace Korpi.Common;

/// <summary>
/// Represents a game server.
/// Has capabilities to handle game logic/state and communicate with clients.
/// </summary>
public interface IGameServer : IDisposable
{
    /// <summary>
    /// Starts the game server using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use</param>
    public void Start(GameServerConfiguration configuration);
}