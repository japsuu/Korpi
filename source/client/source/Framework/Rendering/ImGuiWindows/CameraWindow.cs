using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Rendering.Cameras;
using BlockEngine.Client.Utils;
using ImGuiNET;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Rendering.ImGuiWindows;

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
        ImGui.Text($"Pos: {camPos:F1}");
        ImGui.Text($"Chunk Pos: {CoordinateConversions.WorldToChunk(camPos):F0}");
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
        
        float averageFps = ImGui.GetIO().Framerate;
        
        float frameTime = 1000f / averageFps;
        ImGui.Text($"{averageFps:F1} fps ({frameTime:F1} ms/frame)");
        
        if (Time.FrameCount > CALCULATE_MIN_MAX_FPS_AFTER_FRAMES)
        {
            if (averageFps < _minFps)
                _minFps = averageFps;
            if (averageFps > _maxFps)
                _maxFps = averageFps;
            
            ImGui.Text($"Min: {_minFps:F1} fps");
            ImGui.Text($"Max: {_maxFps:F1} fps");
        }
    }
}