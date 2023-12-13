using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Utils;
using ImGuiNET;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Rendering.ImGuiWindows;

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
        Vector3 camPos = _camera.Transform.Position;
        ImGui.Text($"Pos: {camPos:F1}");
        ImGui.Text($"Chunk Pos: {CoordinateConversions.GetContainingChunkPos(camPos):F0}");
        ImGui.Text($"Pitch: {_camera.Pitch:F1}");
        ImGui.Text($"Yaw: {_camera.Yaw:F1}");
        ImGui.Text($"Fov: {_camera.Fov:F1}");
        ImGui.Text($"Fly spd: {_camera.GetFlySpeedFormatted()}");
        ImGui.Separator();
        ImGui.Text($"Raycast result: {CameraStats.RaycastResult}");
        ImGui.Separator();
        ImGui.Text($"Time: {GameTime.GetFormattedTime()}");
        ImGui.Text($"Date: {GameTime.GetFormattedDate()}");
    }
}