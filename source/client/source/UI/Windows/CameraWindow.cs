using ImGuiNET;
using Korpi.Client.Debugging;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Mathematics;
using Korpi.Client.Registries;
using Korpi.Client.Rendering.Cameras;
using OpenTK.Mathematics;

namespace Korpi.Client.UI.Windows;

public class CameraWindow : ImGuiWindow
{
    public override string Title => "Main Camera";


    public CameraWindow(bool autoRegister = true) : base(autoRegister)
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void DrawContent()
    {
        Vector3 camPos = Camera.RenderingCamera.Position;
        ImGui.Text($"Pos: {camPos:F1}");
        ImGui.Text($"Chunk Pos: {CoordinateUtils.WorldToChunk(camPos):F0}");
        // ImGui.Text($"PitchDegrees: {Camera.RenderingCamera.PitchDegrees:F1}");
        // ImGui.Text($"YawDegrees: {Camera.RenderingCamera.YawDegrees:F1}");
        // ImGui.Text($"FovDegrees: {Camera.RenderingCamera.FovDegrees:F1}");
        if (Camera.RenderingCamera is NoclipCamera noclipCamera)
            ImGui.Text($"Fly spd: {noclipCamera.GetFlySpeedFormatted()}");
        
        ImGui.Separator();
        ImGui.Text("Raycasting");
        ImGui.Text($"Selected block: {BlockRegistry.GetBlock(PlayerEntity.SelectedBlockType)}");
        ImGui.Text($"Raycast result: {DebugStats.LastRaycastResult}");
        
        ImGui.Separator();
        ImGui.Text("Time & Date");
        ImGui.Text($"Time: {GameTime.GetFormattedTime()}");
        ImGui.Text($"Date: {GameTime.GetFormattedDate()}");
        ImGui.Text($"TTime (s): {GameTime.TotalTime:F1}");

#if DEBUG
        ImGui.Separator();
        ImGui.Text("Frustum Culling");
        ImGui.Checkbox("Enabled", ref Configuration.ClientConfig.Rendering.Debug.DoFrustumCulling);
        ImGui.Checkbox("Use player for frustum culling", ref Configuration.ClientConfig.Rendering.Debug.OnlyPlayerFrustumCulling);
#endif
    }
}