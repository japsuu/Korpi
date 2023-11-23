using ImGuiNET;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public static class ImGuiWindowManager
{
    private static readonly List<ImGuiWindow> RegisteredWindows = new();
    
    private static bool shouldRenderWindows = true;
    
    
    public static void CreateDefaultWindows()
    {
        RenderingWindow unused1 = new();
        MemoryProfilerWindow unused2 = new();
        DebugTextWindow unused3 = new();
    }


    public static void RegisterWindow(ImGuiWindow window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        RegisteredWindows.Add(window);
    }


    public static void UnregisterWindow(ImGuiWindow window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        RegisteredWindows.Remove(window);
    }


    public static void UpdateAllWindows()
    {
        ImGui.Begin("Windows", ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Checkbox("Render Windows", ref shouldRenderWindows);
        ImGui.Separator();
        foreach (ImGuiWindow window in RegisteredWindows)
        {
            bool windowVisible = window.IsVisible;
            if (ImGui.Checkbox($"{window.GetType().Name} -> {window.Title}", ref windowVisible))
                window.ToggleVisibility();
        }
        ImGui.End();
        
        if (!shouldRenderWindows)
            return;
        
        foreach (ImGuiWindow window in RegisteredWindows)
            window.Update();
    }
}