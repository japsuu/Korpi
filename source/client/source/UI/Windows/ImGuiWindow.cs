using ImGuiNET;

namespace BlockEngine.Client.UI.Windows;

public abstract class ImGuiWindow
{
    protected ImGuiWindowFlags Flags = ImGuiWindowFlags.None;
    
    public abstract string Title { get; }
    
    public bool IsVisible { get; private set; } = true;


    protected ImGuiWindow()
    {
        ImGuiWindowManager.RegisterWindow(this);
    }


    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    
    public void Update()
    {
        // Only update if the window is visible and the time since the last update exceeds the update rate.
        if (!IsVisible)
            return;
        
        PreUpdate();

        ImGui.Begin(Title, Flags);

        UpdateContent();
        
        ImGui.End();
    }
    
    
    protected virtual void PreUpdate() { }

    
    protected abstract void UpdateContent();
}