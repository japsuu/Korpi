using ImGuiNET;
using Korpi.Client.UI.Windows;

namespace Korpi.Client.UI;

public static class ImGuiWindowManager
{
    private static readonly Dictionary<ImGuiWindow, string> RegisteredWindows = new();
    
    private static bool shouldRenderWindows = true;
    
    
    public static void CreateDefaultWindows()
    {   //BUG: Remove this, as windows are scene-specific
        MemoryProfilerWindow unused = new();
#if DEBUG
        RenderingWindow unused1 = new();
        GenerationWindow unused5 = new();
#endif
        CameraWindow unused2 = new();
        DebugStatsWindow unused4 = new();
        ProfilerWindow unused6 = new();
    }


    public static void RegisterWindow(ImGuiWindow window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        RegisteredWindows.Add(window, window.GetType().Name);
    }


    public static void UnregisterWindow(ImGuiWindow window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        RegisteredWindows.Remove(window);
    }


    public static void DrawAllWindows()
    {
        ImGui.Begin("Windows", ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Checkbox("Draw Windows", ref shouldRenderWindows);
        ImGui.Separator();
        foreach (KeyValuePair<ImGuiWindow, string> kvp in RegisteredWindows)
        {
            bool windowVisible = kvp.Key.IsVisible;
            if (ImGui.Checkbox($"{kvp.Value} -> {kvp.Key.Title}", ref windowVisible))
                kvp.Key.ToggleVisibility();
        }
        ImGui.End();
        
        if (!shouldRenderWindows)
            return;
        
        foreach (ImGuiWindow window in RegisteredWindows.Keys)
            window.Update();
    }


    public static void Dispose()
    {
        foreach (ImGuiWindow window in RegisteredWindows.Keys)
            window.Dispose();
    }
}