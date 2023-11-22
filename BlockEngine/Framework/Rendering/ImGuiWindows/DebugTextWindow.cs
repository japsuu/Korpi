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
    
    private static List<DebugText> frameTexts = null!;
    private static List<DebugText> staticTexts = null!;
    
    
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
        
        frameTexts = new List<DebugText>();
        staticTexts = new List<DebugText>();
    }
    
    
    public static void AddFrameText(Vector3 position, string text)
    {
        frameTexts.Add(new DebugText(position, text));
    }
    
    
    public static void AddStaticText(Vector3 position, string text)
    {
        staticTexts.Add(new DebugText(position, text));
    }
    
    
    public static void RemoveStaticText(string text)
    {
        for (int i = 0; i < staticTexts.Count; i++)
        {
            if (staticTexts[i].Text == text)
            {
                staticTexts.RemoveAt(i);
                return;
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
            foreach (DebugText text in frameTexts)
            {
                if (ShaderManager.WorldPositionToScreenPosition(text.Position, out Vector2 screenPos))
                {
                    ImGui.GetWindowDrawList().AddText(
                        new System.Numerics.Vector2(screenPos.X, screenPos.Y),
                        ImGui.GetColorU32(new System.Numerics.Vector4(0, 0, 0, 1)),
                        $"{text.Text}");
                }
            }
        }
        
        if (staticTexts.Count > 0)
        {
            foreach (DebugText text in staticTexts)
            {
                if (ShaderManager.WorldPositionToScreenPosition(text.Position, out Vector2 screenPos))
                {
                    ImGui.GetWindowDrawList().AddText(
                        new System.Numerics.Vector2(screenPos.X, screenPos.Y),
                        ImGui.GetColorU32(new System.Numerics.Vector4(0, 0, 0, 1)),
                        $"{text.Text}");
                }
            }
        }
        
        frameTexts.Clear();
    }
}