using BlockEngine.Client.Debugging;
using BlockEngine.Client.ECS.Entities;
using BlockEngine.Client.Mathematics;
using BlockEngine.Client.Registries;
using BlockEngine.Client.Rendering.Cameras;
using BlockEngine.Client.Window;
using ImGuiNET;
using OpenTK.Mathematics;

namespace BlockEngine.Client.UI.Windows;

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
        ImGui.Text($"Chunk Pos: {CoordinateUtils.WorldToChunk(camPos):F0}");
        ImGui.Text($"PitchDegrees: {Camera.RenderingCamera.PitchDegrees:F1}");
        ImGui.Text($"YawDegrees: {Camera.RenderingCamera.YawDegrees:F1}");
        ImGui.Text($"FovDegrees: {Camera.RenderingCamera.FovDegrees:F1}");
        if (Camera.RenderingCamera is NoclipCamera noclipCamera)
            ImGui.Text($"Fly spd: {noclipCamera.GetFlySpeedFormatted()}");
        ImGui.Separator();
        ImGui.Text($"Selected block: {BlockRegistry.GetBlock(PlayerEntity.SelectedBlockType)}");
        ImGui.Text($"Raycast result: {CameraWindowData.LastRaycastResult}");
        ImGui.Separator();
        ImGui.Text($"Time: {GameTime.GetFormattedTime()}");
        ImGui.Text($"Date: {GameTime.GetFormattedDate()}");
    }
}