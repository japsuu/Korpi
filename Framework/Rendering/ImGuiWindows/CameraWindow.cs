using ImGuiNET;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public class CameraWindow : ImGuiWindow
{
    private readonly Camera _camera;
    
    public override string Title => "Main Camera";


    public CameraWindow(Camera camera)
    {
        _camera = camera;
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void UpdateContent()
    {
        ImGui.Text($"Position: {_camera.Transform.Position:F1}");
        ImGui.Text($"Pitch: {_camera.Pitch:F1}");
        ImGui.Text($"Yaw: {_camera.Yaw:F1}");
        ImGui.Text($"Fov: {_camera.Fov:F1}");
        ImGui.Text($"Camera fly speed: {_camera.GetFlySpeedFormatted()}");
    }
}