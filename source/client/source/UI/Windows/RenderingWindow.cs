#if DEBUG
using ImGuiNET;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.World;

namespace Korpi.Client.UI.Windows;

public class RenderingWindow : ImGuiWindow
{
    public override string Title => "Rendering Settings";


    public RenderingWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void UpdateContent()
    {
        if (ImGui.Checkbox("Draw chunk borders", ref ClientConfig.Rendering.RenderChunkBorders))
        {
            if (ClientConfig.Rendering.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Draw column borders", ref ClientConfig.Rendering.RenderColumnBorders))
        {
            if (ClientConfig.Rendering.RenderColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        ImGui.Checkbox("Draw chunk mesh state", ref ClientConfig.Rendering.Debug.RenderChunkMeshState);

        ImGui.Separator();
        ImGui.Checkbox("Enable Wireframe", ref ClientConfig.Rendering.Debug.RenderWireframe);
        if (ImGui.Checkbox("Enable Ambient Occlusion", ref ClientConfig.Rendering.Debug.EnableAmbientOcclusion))
            GameWorld.ReloadAllChunks();
        ImGui.Checkbox("Enable skybox", ref ClientConfig.Rendering.Debug.RenderSkybox);
        ImGui.Checkbox("Enable crosshair", ref ClientConfig.Rendering.Debug.RenderCrosshair);

        ImGui.Separator();
        ImGui.Checkbox("Draw raycast path", ref ClientConfig.Rendering.Debug.RenderRaycastPath);
        ImGui.Checkbox("Draw raycast hit", ref ClientConfig.Rendering.Debug.RenderRaycastHit);
        ImGui.Checkbox("Highlight targeted block", ref ClientConfig.Rendering.Debug.HighlightTargetedBlock);
            
        ImGui.Separator();
        ImGui.Text("Time of day");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        float dayProgress = GameTime.DayProgress;
        if (ImGui.SliderFloat("##rendering_day_progress", ref dayProgress, 0.1f, 0.9f))
            GameTime.SetDayProgress(dayProgress);
    }
}
#endif