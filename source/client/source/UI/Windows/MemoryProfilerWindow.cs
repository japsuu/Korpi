using System.Diagnostics;
using ImGuiNET;
using Korpi.Client.Window;

namespace Korpi.Client.UI.Windows;

public class MemoryProfilerWindow : ImGuiWindow
{
    private const int UPDATE_INTERVAL_FRAMES = 60;
    private const long BYTES_TO_MEGABYTES = 1048576L;
    public override string Title => "Memory Profiler";

    private readonly Process _proc;
    private GCMemoryInfo _gcMemoryInfo;
    private int _framesSinceUpdate;


    public MemoryProfilerWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
        _proc = Process.GetCurrentProcess();
        _gcMemoryInfo = GC.GetGCMemoryInfo(GCKind.Any);
    }


    protected override void UpdateContent()
    {
        if (_framesSinceUpdate++ >= UPDATE_INTERVAL_FRAMES)
        {
            _gcMemoryInfo = GC.GetGCMemoryInfo(GCKind.Any);
            _framesSinceUpdate = 0;
        }

        // Add hover tooltip
        ImGui.Text($"GC Alloc. Approx.: {GC.GetTotalMemory(false) / BYTES_TO_MEGABYTES} MB");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("The memory currently thought to be allocated by GC.");
            ImGui.EndTooltip();
        }
        ImGui.Text($"GC Avail.:         {_gcMemoryInfo.TotalAvailableMemoryBytes / BYTES_TO_MEGABYTES} MB");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("The total amount of memory available for GC to use.");
            ImGui.EndTooltip();
        }
        ImGui.Text($"Process Alloc.:    {_proc.PrivateMemorySize64 / BYTES_TO_MEGABYTES} MB");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("The amount of memory allocated for this process.");
            ImGui.EndTooltip();
        }
    }


    public override void Dispose()
    {
        base.Dispose();
        _proc.Dispose();
    }
}