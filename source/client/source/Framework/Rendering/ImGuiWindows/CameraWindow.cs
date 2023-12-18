using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Rendering.Cameras;
using BlockEngine.Client.Utils;
using ImGuiNET;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Rendering.ImGuiWindows;

public class CameraWindow : ImGuiWindow
{
    public override string Title => "Main Camera";


    public CameraWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void UpdateContent()
    {
        Vector3 camPos = Camera.RenderingCamera.Position;
        ImGui.Text($"Pos: {camPos:F1}");
        ImGui.Text($"Chunk Pos: {CoordinateConversions.GetContainingChunkPos(camPos):F0}");
        ImGui.Text($"PitchDegrees: {Camera.RenderingCamera.PitchDegrees:F1}");
        ImGui.Text($"YawDegrees: {Camera.RenderingCamera.YawDegrees:F1}");
        ImGui.Text($"FovDegrees: {Camera.RenderingCamera.FovDegrees:F1}");
        if (Camera.RenderingCamera is NoclipCamera noclipCamera)
            ImGui.Text($"Fly spd: {noclipCamera.GetFlySpeedFormatted()}");
        ImGui.Separator();
        ImGui.Text($"Raycast result: {CameraStats.RaycastResult}");
        ImGui.Separator();
        ImGui.Text($"Time: {GameTime.GetFormattedTime()}");
        ImGui.Text($"Date: {GameTime.GetFormattedDate()}");
    }
}