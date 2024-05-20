#if DEBUG
using ImGuiNET;

namespace Korpi.Client.UI.Windows.Variables;

public class EditorVariableInt : EditorVariable<int>
{
    private readonly int _min;
    private readonly int _max;


    public EditorVariableInt(string name, Action<int> setter, Func<int> getter, int min, int max) : base(name, setter, getter)
    {
        _min = min;
        _max = max;
    }
    
    
    protected override void DrawEditor(int currentValue)
    {
        ImGui.SetNextItemWidth(100);
        if (ImGui.DragInt($"##{Name}", ref currentValue, 1, _min, _max))
            SetValue(currentValue);
    }
}

#endif