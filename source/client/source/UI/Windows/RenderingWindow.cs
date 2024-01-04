#if DEBUG
using BlockEngine.Client.Configuration;
using BlockEngine.Client.Debugging.Drawing;
using BlockEngine.Client.World;
using ImGuiNET;

namespace BlockEngine.Client.UI.Windows;

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

        if (ImGui.Checkbox("Draw region borders", ref ClientConfig.DebugModeConfig.RenderRegionBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderRegionBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        ImGui.Checkbox("Draw chunk mesh state", ref ClientConfig.DebugModeConfig.RenderChunkMeshState);

        ImGui.Separator();
        ImGui.Checkbox("Enable Wireframe", ref ClientConfig.DebugModeConfig.RenderWireframe);
        if (ImGui.Checkbox("Enable Ambient Occlusion", ref ClientConfig.DebugModeConfig.EnableAmbientOcclusion))
            GameWorld.CurrentGameWorld.RegionManager.RemeshAllColumns();
        ImGui.Checkbox("Enable skybox", ref ClientConfig.DebugModeConfig.RenderSkybox);

        ImGui.Separator();
        ImGui.Checkbox("Draw raycast path", ref ClientConfig.DebugModeConfig.RenderRaycastPath);
        ImGui.Checkbox("Draw raycast hit", ref ClientConfig.DebugModeConfig.RenderRaycastHit);
        ImGui.Checkbox("Draw raycast hit block", ref ClientConfig.DebugModeConfig.RenderRaycastHitBlock);
    }
}
#endif