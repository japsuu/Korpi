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
        if (ImGui.Checkbox("Draw chunk borders", ref ClientConfig.Debugging.RenderChunkBorders))
        {
            if (ClientConfig.Debugging.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Draw column borders", ref ClientConfig.Debugging.RenderColumnBorders))
        {
            if (ClientConfig.Debugging.RenderColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        ImGui.Checkbox("Draw chunk mesh state", ref ClientConfig.Debugging.RenderChunkMeshState);

        ImGui.Separator();
        ImGui.Checkbox("Enable Wireframe", ref ClientConfig.Debugging.RenderWireframe);
        if (ImGui.Checkbox("Enable Ambient Occlusion", ref ClientConfig.Debugging.EnableAmbientOcclusion))
            GameWorld.ReloadAllChunks();
        ImGui.Checkbox("Enable skybox", ref ClientConfig.Debugging.RenderSkybox);
        ImGui.Checkbox("Enable crosshair", ref ClientConfig.Debugging.RenderCrosshair);

        ImGui.Separator();
        ImGui.Checkbox("Draw raycast path", ref ClientConfig.Debugging.RenderRaycastPath);
        ImGui.Checkbox("Draw raycast hit", ref ClientConfig.Debugging.RenderRaycastHit);
        ImGui.Checkbox("Draw raycast hit block", ref ClientConfig.Debugging.RenderRaycastHitBlock);
            
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