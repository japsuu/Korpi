using System.Globalization;
using ImGuiNET;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Rendering.Chunks;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.Window;

namespace Korpi.Client.UI.Windows;

public class DebugStatsWindow : ImGuiWindow
{
    private const int CALCULATE_MIN_MAX_FPS_AFTER_FRAMES = 1000;
    
    public override string Title => "Debug Stats";

    private readonly NumberFormatInfo _largeNumberFormat;

    private float _minFps = float.MaxValue;
    private float _maxFps = float.MinValue;


    public DebugStatsWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
        _largeNumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _largeNumberFormat.NumberGroupSeparator = " ";
    }


    protected override void UpdateContent()
    {
        uint loadedChunksApprox = (uint)DebugStats.LoadedRegionCount * Constants.CHUNK_HEIGHT_SUBCHUNKS;
        const uint chunkSizeCubed = Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH;
        uint loadedBlocksApprox = loadedChunksApprox * chunkSizeCubed;
        
        string loadedBlocksApproxFormatted = loadedBlocksApprox.ToString("#,0", _largeNumberFormat);
        string loadedChunksApproxFormatted = loadedChunksApprox.ToString("#,0", _largeNumberFormat);
        string renderedTris = DebugStats.RenderedTris.ToString("#,0", _largeNumberFormat);
        
        float averageFps = ImGui.GetIO().Framerate;
        float frameTime = 1000f / averageFps;
        bool shouldCalcMinMaxFps = GameTime.TotalFrameCount > CALCULATE_MIN_MAX_FPS_AFTER_FRAMES;

        ImGui.Text("Rendering");
        ImGui.Text($"{averageFps:F1} fps ({frameTime:F1} ms/frame)");
        if (shouldCalcMinMaxFps)
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
        ImGui.Text($"Loaded blocks (approx) = {loadedBlocksApproxFormatted}");
        ImGui.Text($"Loaded chunks (approx) = {loadedChunksApproxFormatted}");
        ImGui.Text($"Loaded regions = {DebugStats.LoadedRegionCount}");

        ImGui.Separator();
        ImGui.Text("Global Thread Pool");
        ImGui.Text($"Threads = {GlobalThreadPool.ThreadCount}");
        ImGui.Text($"In queue = {DebugStats.ItemsInMainThreadQueue}");
        ImGui.Text($"In queue (throttled) = {DebugStats.ItemsInMainThreadThrottledQueue}");
        ImGui.Text($"Throttled items per tick = {DebugStats.MainThreadThrottledQueueItemsPerTick}");

        ImGui.Separator();
        ImGui.Text("Chunk Generation");
        ImGui.Text($"Queued = {DebugStats.ChunksInGenerationQueue}");
        ImGui.Text($"Average gen time = {DebugStats.AverageChunkGenerationTime:F1}ms");
        ImGui.Text($"Median gen time = {DebugStats.MedianChunkGenerationTime:F1}ms");
        ImGui.Text($"Min/Max gen time = {DebugStats.MinChunkGenerationTime:F1}/{DebugStats.MaxChunkGenerationTime:F1}ms");

        ImGui.Separator();
        ImGui.Text("Chunk Meshing");
        ImGui.Text($"Queued = {DebugStats.ChunksInMeshingQueue}");
        ImGui.Text($"Average mesh time = {DebugStats.AverageChunkMeshingTime:F1}ms");
        ImGui.Text($"Median mesh time = {DebugStats.MedianChunkMeshingTime:F1}ms");
        ImGui.Text($"Min/Max mesh time = {DebugStats.MinChunkMeshingTime:F1}/{DebugStats.MaxChunkMeshingTime:F1}ms");
        ImGui.Text($"Active meshes = {ChunkRendererStorage.GeneratedRendererCount}");
    }
}