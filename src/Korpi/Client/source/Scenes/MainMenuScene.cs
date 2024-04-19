using Korpi.Client.UI.Windows;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.UI.ImGui;

namespace Korpi.Client.Scenes;

public class MainMenuScene : Scene
{
    private readonly GameClient _client;
    private readonly ConnectionWindow _connectionWindow;
    
    public MainMenuScene(GameClient client)
    {
        _client = client;
        _connectionWindow = new ConnectionWindow(false);
        _connectionWindow.ConnectClicked += _client.ConnectToServer;
        _connectionWindow.HostClicked += _client.StartLocalServer;
        
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
        SceneManager.LoadScene(new GameScene(_client));
    }
}