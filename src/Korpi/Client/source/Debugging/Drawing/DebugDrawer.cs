using Korpi.Client.Debugging.Drawing.Drawables;
using Korpi.Client.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Debugging.Drawing;

public static class DebugDrawer
{
    private static readonly List<DebugDrawable> Drawables = new();
    private static readonly List<DebugDrawable> TemporaryDrawables = new();
    
    public static readonly int VBO;
    public static readonly int VAO;

    
    static DebugDrawer()
    {
        VAO = GL.GenVertexArray();
        VBO = GL.GenBuffer();
    }
    

    public static void DrawLine(Vector3 start, Vector3 end, Color4 color)
    {
        Drawables.Add(new DebugLine(start, end, color, 0f));
    }
    

    public static void DrawLineTemporary(Vector3 start, Vector3 end, Color4 color, float lifetimeSeconds)
    {
        TemporaryDrawables.Add(new DebugLine(start, end, color, lifetimeSeconds));
    }
    
    
    public static void DrawBox(Vector3 position, Vector3 size, Color4 color)
    {
        Drawables.Add(new DebugBox(position, size, color, 0f));
    }
    
    
    public static void DrawBoxTemporary(Vector3 position, Vector3 size, Color4 color, float lifetimeSeconds)
    {
        TemporaryDrawables.Add(new DebugBox(position, size, color, lifetimeSeconds));
    }
    
    
    public static void DrawSphere(Vector3 position, float radius, Color4 color)
    {
        Drawables.Add(new DebugSphere(position, radius, color, 0f));
    }
    
    
    public static void DrawSphereTemporary(Vector3 position, float radius, Color4 color, float lifetimeSeconds)
    {
        TemporaryDrawables.Add(new DebugSphere(position, radius, color, lifetimeSeconds));
    }
    

    public static void Draw()
    {
        ShaderManager.PositionColorShader.Use();

        foreach (DebugDrawable drawable in Drawables)
            drawable.DrawObject();

        Drawables.Clear();

        List<DebugDrawable> drawablesToRemove = new();
        foreach (DebugDrawable drawable in TemporaryDrawables)
        {
            if (drawable.LifetimeSeconds <= 0f)
                drawablesToRemove.Add(drawable);
            else
                drawable.DrawObject();

            drawable.LifetimeSeconds -= (float)GameTime.DeltaTime;
        }

        foreach (DebugDrawable drawable in drawablesToRemove)
            TemporaryDrawables.Remove(drawable);
    }
}