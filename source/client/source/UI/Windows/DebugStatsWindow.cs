using System.Globalization;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Rendering.Chunks;
using BlockEngine.Client.Window;
using ImGuiNET;

namespace BlockEngine.Client.UI.Windows;

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
        uint loadedChunksApprox = (uint)DebugStats.LoadedColumnCount * Constants.CHUNK_COLUMN_HEIGHT;
        uint loadedBlocksApprox = loadedChunksApprox * Constants.CHUNK_SIZE_CUBED;
        
        string loadedBlocksApproxFormatted = loadedBlocksApprox.ToString("#,0", _largeNumberFormat);
        string loadedChunksApproxFormatted = loadedChunksApprox.ToString("#,0", _largeNumberFormat);
        
        float averageFps = ImGui.GetIO().Framerate;
        float frameTime = 1000f / averageFps;
        bool shouldCalcMinMaxFps = GameTime.TotalFrameCount > CALCULATE_MIN_MAX_FPS_AFTER_FRAMES;
        
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

        ImGui.Text($"Loaded Blocks (approx) = {loadedBlocksApproxFormatted}");
        ImGui.Text($"Loaded Chunks (approx) = {loadedChunksApproxFormatted}");
        ImGui.Text($"Loaded Columns = {DebugStats.LoadedColumnCount}");

        ImGui.Separator();
        ImGui.Text("Chunk Generation");
        ImGui.Text($"Chunks in generation queue = {DebugStats.ChunksInGenerationQueue}");
        ImGui.Text($"Average chunk generation time = {DebugStats.AverageChunkGenerationTime:F1}ms");

        ImGui.Separator();
        ImGui.Text("Chunk Meshing");
        ImGui.Text($"Chunks in meshing queue = {DebugStats.ChunksInMeshingQueue}");
        ImGui.Text($"Average chunk meshing time = {DebugStats.AverageChunkMeshingTime:F1}ms");
        ImGui.Text($"Active chunk meshes = {ChunkRendererStorage.GeneratedRendererCount}");
    }
}