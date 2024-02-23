namespace Korpi.Client.SceneManagement.Scenes;

public class GameScene : Scene
{
    public GameScene(GameClient client) : base(client)
    {
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
        SceneManager.LoadScene(new MainMenuScene(Client));
    }
}