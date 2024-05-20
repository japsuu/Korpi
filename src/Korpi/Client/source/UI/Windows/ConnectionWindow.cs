using ImGuiNET;
using Korpi.Client.Multiplayer;
using KorpiEngine.Core.UI.ImGui;

namespace Korpi.Client.UI.Windows;

/// <summary>
/// Allows the user to join or host a game.
/// </summary>
public class ConnectionWindow : ImGuiWindow
{
    public override string Title => "Join/Host Game";
    
    public event Action<ServerConnectInfo>? ConnectClicked;
    public event Action? HostClicked;

    private string _ipAddress = "0.0.0.0";
    private int _port = 7531;


    public ConnectionWindow(bool autoRegister) : base(autoRegister)
    {
    }

    
    protected override void DrawContent()
    {
        ImGui.InputText("IP Address", ref _ipAddress, 16);
        ImGui.SliderInt("Port", ref _port, short.MinValue, short.MaxValue);
        
        if (ImGui.Button("Connect"))
            ConnectClicked?.Invoke(new ServerConnectInfo(_ipAddress, (ushort)_port));
        
        if (ImGui.Button("Host"))
            HostClicked?.Invoke();
    }
}