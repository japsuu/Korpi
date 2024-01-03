#if DEBUG
using System.Globalization;
using BlockEngine.Client.Configuration;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Debugging.Drawing;
using BlockEngine.Client.Rendering.Chunks;
using BlockEngine.Client.World;
using ImGuiNET;

namespace BlockEngine.Client.UI.ImGui.Windows;

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
        if (ImGuiNET.ImGui.Checkbox("Draw chunk borders", ref ClientConfig.DebugModeConfig.RenderChunkBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGuiNET.ImGui.Checkbox("Draw column borders", ref ClientConfig.DebugModeConfig.RenderChunkColumnBorders))
        {
            if (ClientConfig.DebugModeConfig.RenderChunkColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        ImGuiNET.ImGui.Checkbox("Draw chunk mesh state", ref ClientConfig.DebugModeConfig.RenderChunkMeshState);

        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Checkbox("Enable Wireframe", ref ClientConfig.DebugModeConfig.RenderWireframe);
        if (ImGuiNET.ImGui.Checkbox("Enable Ambient Occlusion", ref ClientConfig.DebugModeConfig.EnableAmbientOcclusion))
            GameWorld.CurrentGameWorld.ChunkManager.RemeshAllColumns();
        ImGuiNET.ImGui.Checkbox("Enable skybox", ref ClientConfig.DebugModeConfig.RenderSkybox);

        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Checkbox("Draw raycast path", ref ClientConfig.DebugModeConfig.RenderRaycastPath);
        ImGuiNET.ImGui.Checkbox("Draw raycast hit", ref ClientConfig.DebugModeConfig.RenderRaycastHit);
        ImGuiNET.ImGui.Checkbox("Draw raycast hit block", ref ClientConfig.DebugModeConfig.RenderRaycastHitBlock);

        ImGuiNET.ImGui.Separator();
        uint loadedChunks = (uint)RenderingWindowData.LoadedColumnCount * Constants.CHUNK_COLUMN_HEIGHT;
        ImGuiNET.ImGui.Text($"Loaded Blocks = {(loadedChunks * Constants.CHUNK_SIZE_CUBED).ToString("#,0", _numberFormat)}");
        ImGuiNET.ImGui.Text($"Loaded Chunks = {loadedChunks.ToString("#,0", _numberFormat)}");
        ImGuiNET.ImGui.Text($"Loaded Columns = {RenderingWindowData.LoadedColumnCount}");

        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Text("Chunk Generation");
        ImGuiNET.ImGui.Text($"Chunks in generation queue = {RenderingWindowData.ChunksInGenerationQueue}");
        ImGuiNET.ImGui.Text($"Average chunk generation time = {RenderingWindowData.AverageChunkGenerationTime:F1}ms");

        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Text("Chunk Meshing");
        ImGuiNET.ImGui.Text($"Chunks in meshing queue = {RenderingWindowData.ChunksInMeshingQueue}");
        ImGuiNET.ImGui.Text($"Average chunk meshing time = {RenderingWindowData.AverageChunkMeshingTime:F1}ms");
        ImGuiNET.ImGui.Text($"Active chunk meshes = {ChunkRendererStorage.GeneratedRendererCount}");
    }
}
#endif