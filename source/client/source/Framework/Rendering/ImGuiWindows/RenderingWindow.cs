#if DEBUG
using System.Globalization;
using BlockEngine.Client.Framework.Configuration;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Meshing;
using ImGuiNET;

namespace BlockEngine.Client.Framework.Rendering.ImGuiWindows;

public class RenderingWindow : ImGuiWindow
{
    public override string Title => "Rendering Settings";

    private readonly NumberFormatInfo _numberFormat;


    public RenderingWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
        _numberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _numberFormat.NumberGroupSeparator = " ";
    }


    protected override void UpdateContent()
    {
        ImGui.Checkbox("Wireframe rendering", ref ClientConfig.DebugModeConfig.RenderWireframe);

        if (ImGui.Checkbox("Draw chunk borders", ref ClientConfig.DebugModeConfig.RenderChunkBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Draw column borders", ref ClientConfig.DebugModeConfig.RenderChunkColumnBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderChunkColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Enable Ambient Occlusion", ref ClientConfig.DebugModeConfig.EnableAmbientOcclusion))
        {
            World.CurrentWorld.ChunkManager.ReloadAllChunks();
        }
        
        ImGui.Checkbox("Draw skybox", ref ClientConfig.DebugModeConfig.RenderSkybox);

        ImGui.Separator();
        
        ImGui.Checkbox("Draw raycast path", ref ClientConfig.DebugModeConfig.RenderRaycastPath);
        ImGui.Checkbox("Draw raycast hit", ref ClientConfig.DebugModeConfig.RenderRaycastHit);
        ImGui.Checkbox("Draw raycast hit block", ref ClientConfig.DebugModeConfig.RenderRaycastHitBlock);

        ImGui.Separator();
        
        uint loadedChunks = (uint)RenderingStats.LoadedColumnCount * Constants.CHUNK_COLUMN_HEIGHT;
        ImGui.Text($"Loaded Blocks = {(loadedChunks * Constants.CHUNK_SIZE_CUBED).ToString("#,0", _numberFormat)}");
        ImGui.Text($"Loaded Chunks = {loadedChunks.ToString("#,0", _numberFormat)}");
        ImGui.Text($"Loaded Columns = {RenderingStats.LoadedColumnCount}");
        ImGui.Text($"Cached chunk meshes = {ChunkRendererStorage.GeneratedRendererCount}");
        ImGui.Text($"Chunks in generation queue = {RenderingStats.ChunksInGenerationQueue}");
        ImGui.Text($"Chunks in meshing queue = {RenderingStats.ChunksInMeshingQueue}");
        ImGui.Text($"Meshing queue processing time = {RenderingStats.MeshingQueueProcessingTime:F1}ms");
        ImGui.Text($"Average chunk meshing time = {RenderingStats.AverageChunkMeshingTime:F1}ms");
    }
}
#endif