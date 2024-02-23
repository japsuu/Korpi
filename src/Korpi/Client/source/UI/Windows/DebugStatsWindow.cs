using System.Globalization;
using ImGuiNET;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;

namespace Korpi.Client.UI.Windows;

public class DebugStatsWindow : ImGuiWindow
{
    public override string Title => "Debug Stats";

    private readonly NumberFormatInfo _largeNumberFormat;

    private bool _shouldCalcMinMaxFps;
    private float _minFps = float.MaxValue;
    private float _maxFps = float.MinValue;


    public DebugStatsWindow(bool autoRegister = true) : base(autoRegister)
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
        _largeNumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _largeNumberFormat.NumberGroupSeparator = " ";
    }


    protected override void DrawContent()
    {
        uint loadedChunksApprox = (uint)DebugStats.LoadedChunkCount * Constants.CHUNK_COLUMN_HEIGHT_CHUNKS;
        const uint chunkSizeCubed = Constants.CHUNK_SIDE_LENGTH * Constants.CHUNK_SIDE_LENGTH * Constants.CHUNK_SIDE_LENGTH;
        uint loadedBlocksApprox = loadedChunksApprox * chunkSizeCubed;
        
        string loadedBlocksApproxFormatted = loadedBlocksApprox.ToString("#,0", _largeNumberFormat);
        string loadedChunksApproxFormatted = loadedChunksApprox.ToString("#,0", _largeNumberFormat);
        string renderedTris = DebugStats.RenderedTris.ToString("#,0", _largeNumberFormat);
        
        float averageFps = ImGui.GetIO().Framerate;
        float frameTime = 1000f / averageFps;
        if (ImGui.Checkbox("Calculate min/max FPS", ref _shouldCalcMinMaxFps))
        {
            _minFps = float.MaxValue;
            _maxFps = float.MinValue;
        }

        ImGui.Text("Rendering");
        ImGui.Text($"{averageFps:F1} fps ({frameTime:F1} ms/frame)");
        if (_shouldCalcMinMaxFps)
        {
            if (averageFps < _minFps)
                _minFps = averageFps;
            if (averageFps > _maxFps)
                _maxFps = averageFps;
            
            ImGui.Text($"Min: {_minFps:F1} fps");
            ImGui.Text($"Max: {_maxFps:F1} fps");
        }
        ImGui.Text($"Triangles = {renderedTris}");

        ImGui.Separator();
        ImGui.Text("Loaded regions");
        ImGui.Text($"Loaded columns = {DebugStats.LoadedChunkCount}");
        ImGui.Text($"Loaded chunks = {loadedChunksApproxFormatted}");
        ImGui.Text($"Loaded blocks (approx) = {loadedBlocksApproxFormatted}");

        ImGui.Separator();
        ImGui.Text("Global Thread Pool");
        ImGui.Text($"In queue = {DebugStats.ItemsInMainThreadQueue}");
        ImGui.Text($"In queue (throttled) = {DebugStats.ItemsInMainThreadThrottledQueue}");
        ImGui.Text($"Throttled per tick = {DebugStats.MainThreadThrottledQueueItemsPerTick}");

        ImGui.Separator();
        ImGui.Text("Chunk Generation");
        ImGui.Text($"Waiting = {DebugStats.ChunksWaitingGeneration}");
        ImGui.Text($"Average gen time = {DebugStats.AverageChunkGenerationTime:F1}ms");
        ImGui.Text($"Median gen time = {DebugStats.MedianChunkGenerationTime:F1}ms");
        ImGui.Text($"Min/Max gen time = {DebugStats.MinChunkGenerationTime:F1}/{DebugStats.MaxChunkGenerationTime:F1}ms");

        ImGui.Separator();
        ImGui.Text("Chunk Meshing");
        ImGui.Text($"Waiting = {DebugStats.ChunksWaitingMeshing}");
        ImGui.Text($"Average mesh time = {DebugStats.AverageChunkMeshingTime:F1}ms");
        ImGui.Text($"Median mesh time = {DebugStats.MedianChunkMeshingTime:F1}ms");
        ImGui.Text($"Min/Max mesh time = {DebugStats.MinChunkMeshingTime:F1}/{DebugStats.MaxChunkMeshingTime:F1}ms");
    }
}