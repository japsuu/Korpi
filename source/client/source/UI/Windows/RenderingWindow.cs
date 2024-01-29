#if DEBUG
using ImGuiNET;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Window;
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
        if (ImGui.Checkbox("Draw chunk borders", ref ClientConfig.DebugModeConfig.RenderChunkBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Draw column borders", ref ClientConfig.DebugModeConfig.RenderColumnBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        ImGui.Checkbox("Draw chunk mesh state", ref ClientConfig.DebugModeConfig.RenderChunkMeshState);

        ImGui.Separator();
        ImGui.Checkbox("Enable Wireframe", ref ClientConfig.DebugModeConfig.RenderWireframe);
        if (ImGui.Checkbox("Enable Ambient Occlusion", ref ClientConfig.DebugModeConfig.EnableAmbientOcclusion))
            GameWorld.ReloadAllChunks();
        ImGui.Checkbox("Enable skybox", ref ClientConfig.DebugModeConfig.RenderSkybox);
        ImGui.Checkbox("Enable crosshair", ref ClientConfig.DebugModeConfig.RenderCrosshair);

        ImGui.Separator();
        ImGui.Checkbox("Draw raycast path", ref ClientConfig.DebugModeConfig.RenderRaycastPath);
        ImGui.Checkbox("Draw raycast hit", ref ClientConfig.DebugModeConfig.RenderRaycastHit);
        ImGui.Checkbox("Draw raycast hit block", ref ClientConfig.DebugModeConfig.RenderRaycastHitBlock);
            
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