using KorpiEngine.Core.SceneManagement;

namespace Korpi.Client.Scenes;

public class GameScene : Scene
{
    private readonly GameClient _client;


    public GameScene(GameClient client)
    {
        _client = client;
        client.DisconnectedFromServer += OnDisconnectedFromServer;
    }


    public override void Load()
    {
        
    }


    public override void Unload()
    {
        
    }


    public override void Update()
    {
        
    }


    public override void FixedUpdate()
    {
        
    }


    public override void Draw()
    {
        
    }


    private void OnDisconnectedFromServer()
    {
        SceneManager.LoadScene(new MainMenuScene(_client));
    }
}