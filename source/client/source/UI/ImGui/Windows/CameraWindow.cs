using BlockEngine.Client.Configuration;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Math;
using BlockEngine.Client.Registries;
using BlockEngine.Client.Rendering.Cameras;
using BlockEngine.Client.Window;
using ImGuiNET;
using OpenTK.Mathematics;

namespace BlockEngine.Client.UI.ImGui.Windows;

public class CameraWindow : ImGuiWindow
{
    private const int CALCULATE_MIN_MAX_FPS_AFTER_FRAMES = 1000;
    
    public override string Title => "Main Camera";

    private float _minFps = float.MaxValue;
    private float _maxFps = float.MinValue;


    public CameraWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void UpdateContent()
    {
        Vector3 camPos = Camera.RenderingCamera.Position;
        ImGuiNET.ImGui.Text($"Pos: {camPos:F1}");
        ImGuiNET.ImGui.Text($"Chunk Pos: {CoordinateUtils.WorldToChunk(camPos):F0}");
        ImGuiNET.ImGui.Text($"PitchDegrees: {Camera.RenderingCamera.PitchDegrees:F1}");
        ImGuiNET.ImGui.Text($"YawDegrees: {Camera.RenderingCamera.YawDegrees:F1}");
        ImGuiNET.ImGui.Text($"FovDegrees: {Camera.RenderingCamera.FovDegrees:F1}");
        if (Camera.RenderingCamera is NoclipCamera noclipCamera)
            ImGuiNET.ImGui.Text($"Fly spd: {noclipCamera.GetFlySpeedFormatted()}");
        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Text($"Selected block: {BlockRegistry.GetBlock(ClientConfig.DebugModeConfig.SelectedBlockType)}");
        ImGuiNET.ImGui.Text($"Raycast result: {CameraWindowData.LastRaycastResult}");
        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Text($"Time: {GameTime.GetFormattedTime()}");
        ImGuiNET.ImGui.Text($"Date: {GameTime.GetFormattedDate()}");
        
        float averageFps = ImGuiNET.ImGui.GetIO().Framerate;
        
        float frameTime = 1000f / averageFps;
        ImGuiNET.ImGui.Text($"{averageFps:F1} fps ({frameTime:F1} ms/frame)");
        
        if (GameTime.TotalFrameCount > CALCULATE_MIN_MAX_FPS_AFTER_FRAMES)
        {
            if (averageFps < _minFps)
                _minFps = averageFps;
            if (averageFps > _maxFps)
                _maxFps = averageFps;
            
            ImGuiNET.ImGui.Text($"Min: {_minFps:F1} fps");
            ImGuiNET.ImGui.Text($"Max: {_maxFps:F1} fps");
        }
    }
}