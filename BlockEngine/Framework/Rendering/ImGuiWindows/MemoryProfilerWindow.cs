using System.Diagnostics;
using ImGuiNET;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public class MemoryProfilerWindow : ImGuiWindow
{
    private const long BYTES_TO_MEGABYTES = 1048576L;
    public override string Title => "Memory Profiler";

    private readonly Process _proc;


    public MemoryProfilerWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
        _proc = Process.GetCurrentProcess();
        GameClient.ClientUnload += OnUnload;
    }


    protected override void UpdateContent()
    {
        // Add hover tooltip
        ImGui.Text($"GC Alloc. Approx.: {GC.GetTotalMemory(false) / BYTES_TO_MEGABYTES} MB");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("The memory currently thought to be allocated by GC.");
            ImGui.EndTooltip();
        }
        ImGui.Text($"GC Avail.:         {GC.GetGCMemoryInfo(GCKind.Any).TotalAvailableMemoryBytes / BYTES_TO_MEGABYTES} MB");
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


    private void OnUnload()
    {
        _proc.Dispose();
    }
}