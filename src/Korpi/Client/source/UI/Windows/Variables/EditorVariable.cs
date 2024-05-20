#if DEBUG
using ImGuiNET;

namespace Korpi.Client.UI.Windows.Variables;

public abstract class EditorVariable<T> : IEditorVariable
{
    protected readonly string Name;
    private readonly Action<T> _setter;
    private readonly Func<T> _getter;
    private readonly T _defaultValue;


    public EditorVariable(string name, Action<T> setter, Func<T> getter)
    {
        Name = name;
        _setter = setter;
        _getter = getter;
        _defaultValue = getter();
    }


    public void Draw()
    {
        ImGui.Text(Name);
        ImGui.SameLine();
        DrawEditor(_getter());
        ImGui.SameLine();
        DrawDefaultButton(_defaultValue);
    }


    protected void SetValue(T value) => _setter(value);


    protected abstract void DrawEditor(T currentValue);
    
    
    private void DrawDefaultButton(T defaultValue)
    {
        if (ImGui.Button($"Default##{Name}"))
            SetValue(defaultValue);
    }
}
#endif