using ImGuiNET;

namespace Korpi.Client.UI.Windows;

public abstract class ImGuiWindow
{
    protected static readonly Logging.IKorpiLogger Logger = Logging.LogFactory.GetLogger(typeof(ImGuiWindow));
    
    protected ImGuiWindowFlags Flags = ImGuiWindowFlags.None;
    
    public abstract string Title { get; }
    
    public bool IsVisible { get; protected set; } = true;


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
    
    
    public virtual void Dispose() { }

    
    protected abstract void UpdateContent();
}