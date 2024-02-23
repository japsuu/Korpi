using Korpi.Client.UI;
using Korpi.Client.UI.Windows;

namespace Korpi.Client.SceneManagement.Scenes;

public class MainMenuScene : Scene
{
    private readonly ConnectionWindow _connectionWindow;
    
    public MainMenuScene(GameClient client) : base(client)
    {
        _connectionWindow = new ConnectionWindow(false);
        _connectionWindow.ConnectClicked += Client.ConnectToServer;
        _connectionWindow.HostClicked += Client.StartLocalServer;
        
        client.ConnectedToServer += OnConnectedToServer;
    }


    public override void Load()
    {
        ImGuiWindowManager.RegisterWindow(_connectionWindow);
    }


    public override void Unload()
    {
        ImGuiWindowManager.UnregisterWindow(_connectionWindow);
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


    private void OnConnectedToServer()
    {
        SceneManager.LoadScene(new GameScene(Client));
    }
}