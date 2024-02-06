using Korpi.Common;

namespace Korpi.Server;

public static class GameServerStarter
{
    public static IGameServer StartGameServer(GameServerConfiguration configuration)
    {
        IGameServer server = new GameServer();
        server.Start(configuration);
        return server;
    }
}