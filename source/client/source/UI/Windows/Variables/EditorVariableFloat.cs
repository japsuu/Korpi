#if DEBUG
using ImGuiNET;

namespace Korpi.Client.UI.Windows.Variables;

public class EditorVariableFloat : EditorVariable<float>
{
    private readonly float _min;
    private readonly float _max;


    public EditorVariableFloat(string name, Action<float> setter, Func<float> getter, float min, float max) : base(name, setter, getter)
    {
        _min = min;
        _max = max;
    }
    
    
    protected override void DrawEditor(float currentValue)
    {
        ImGui.SetNextItemWidth(100);
        if (ImGui.DragFloat($"##{Name}", ref currentValue, 1, _min, _max))
            SetValue(currentValue);
    }
}
#endif