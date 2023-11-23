using BlockEngine.Framework.Rendering.Shaders;
using ImGuiNET;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public class DebugTextWindow : ImGuiWindow
{
    private struct DebugText
    {
        public Vector3 Position;
        public string Text;


        public DebugText(Vector3 position, string text)
        {
            Position = position;
            Text = text;
        }
    }
    
    public override string Title => "Debug Text";
    
    private static Dictionary<Vector3, List<DebugText>> frameTexts = null!;
    private static Dictionary<Vector3, List<DebugText>> staticTexts = null!;
    
    
    public DebugTextWindow()
    {
        Flags = 
            ImGuiWindowFlags.NoDecoration | 
            ImGuiWindowFlags.NoMove | 
            ImGuiWindowFlags.NoSavedSettings | 
            ImGuiWindowFlags.NoInputs | 
            ImGuiWindowFlags.NoBackground | 
            ImGuiWindowFlags.NoFocusOnAppearing | 
            ImGuiWindowFlags.NoBringToFrontOnFocus | 
            ImGuiWindowFlags.NoNavFocus | 
            ImGuiWindowFlags.NoNavInputs | 
            ImGuiWindowFlags.NoNav | 
            ImGuiWindowFlags.NoMouseInputs | 
            ImGuiWindowFlags.NoDocking | 
            ImGuiWindowFlags.NoScrollbar | 
            ImGuiWindowFlags.NoScrollWithMouse | 
            ImGuiWindowFlags.NoTitleBar | 
            ImGuiWindowFlags.NoResize | 
            ImGuiWindowFlags.NoCollapse;

        frameTexts = new Dictionary<Vector3, List<DebugText>>();
        staticTexts = new Dictionary<Vector3, List<DebugText>>();
    }
    
    
    public static void AddFrameText(Vector3 position, string text)
    {
        if (!frameTexts.ContainsKey(position))
            frameTexts.Add(position, new List<DebugText>());

        frameTexts[position].Add(new DebugText(position, text));
    }
    
    
    public static void AddStaticText(Vector3 position, string text)
    {
        if (staticTexts == null)
            return;
        
        if (!staticTexts.ContainsKey(position))
            staticTexts.Add(position, new List<DebugText>());

        staticTexts[position].Add(new DebugText(position, text));
    }
    
    
    public static void RemoveStaticText(string text)
    {
        foreach (KeyValuePair<Vector3, List<DebugText>> pair in staticTexts)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                if (pair.Value[i].Text == text)
                {
                    pair.Value.RemoveAt(i);
                    return;
                }
            }
        }
    }


    protected override void PreUpdate()
    {
        ImGui.SetWindowSize(Title, new System.Numerics.Vector2(ShaderManager.WindowWidth, ShaderManager.WindowHeight));
        ImGui.SetWindowPos(Title, new System.Numerics.Vector2(0, 0));
    }


    protected override void UpdateContent()
    {
        if (frameTexts.Count > 0)
        {
            foreach (KeyValuePair<Vector3, List<DebugText>> pair in frameTexts)
            {
                const float heightOffset = 15;
                int index = 0;
                foreach (DebugText text in pair.Value)
                {
                    if (ShaderManager.WorldPositionToScreenPosition(text.Position, out Vector2 screenPos))
                    {
                        System.Numerics.Vector4 color = index % 2 == 0 ? new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1) : new System.Numerics.Vector4(0, 0, 0, 1);
                        ImGui.GetWindowDrawList().AddText(
                            new System.Numerics.Vector2(screenPos.X, screenPos.Y + heightOffset * index),
                            ImGui.GetColorU32(color),
                            $"{text.Text}");
                        index++;
                    }
                }
            }
        }
        
        if (staticTexts.Count > 0)
        {
            foreach (KeyValuePair<Vector3, List<DebugText>> pair in staticTexts)
            {
                const float heightOffset = 15;
                int index = 0;
                foreach (DebugText text in pair.Value)
                {
                    if (ShaderManager.WorldPositionToScreenPosition(text.Position, out Vector2 screenPos))
                    {
                        System.Numerics.Vector4 color = index % 2 == 0 ? new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1) : new System.Numerics.Vector4(0, 0, 0, 1);
                        ImGui.GetWindowDrawList().AddText(
                            new System.Numerics.Vector2(screenPos.X, screenPos.Y + heightOffset * index),
                            ImGui.GetColorU32(color),
                            $"{text.Text}");
                        index++;
                    }
                }
            }
        }
        
        frameTexts.Clear();
    }
}