using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Debugging;

public abstract class DebugDrawable
{
    protected abstract bool UseVertexColors { get; }
    protected abstract Matrix4 ModelMatrix { get; }
    
    protected Color4 Color;
    
    public float LifetimeSeconds;
    
    
    public void DrawObject()
    {
        ShaderManager.DebugShader.SetMatrix4("model", ModelMatrix);
        if (UseVertexColors)
        {
            ShaderManager.DebugShader.SetVector3("overrideColor", new Vector3(1, 1, 1));
            Draw();
        }
        else
        {
            ShaderManager.DebugShader.SetVector3("overrideColor", new Vector3(Color.R, Color.G, Color.B));
            Draw();
        }
    }


    protected abstract void Draw();
}


public class DebugLine : DebugDrawable
{
    private Vector3 Start { get; }
    private Vector3 End { get; }
    private float[] Vertices { get; }
    
    protected override bool UseVertexColors => true;
    protected override Matrix4 ModelMatrix { get; }


    public DebugLine(Vector3 start, Vector3 end, Color4 color, float lifetimeSeconds)
    {
        Start = start;
        End = end;
        Color = color;
        LifetimeSeconds = lifetimeSeconds;
        ModelMatrix = Matrix4.Identity;

        Vertices = new[]
        {
            Start.X, Start.Y, Start.Z, Color.R, Color.G, Color.B,
            End.X, End.Y, End.Z, Color.R, Color.G, Color.B
        };
    }


    protected override void Draw()
    {
        GL.BindVertexArray(DebugDrawer.VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, DebugDrawer.VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.DrawArrays(PrimitiveType.Lines, 0, 2);
    }
}


public class DebugBox : DebugDrawable
{
    private Vector3 Position { get; }
    private Vector3 Size { get; }
    private static readonly float[] Vertices = {
        // Front face
        -0.5f, -0.5f, 0.5f, 1, 1, 1,
        0.5f, -0.5f, 0.5f, 1, 1, 1,
        0.5f, 0.5f, 0.5f, 1, 1, 1,
        -0.5f, 0.5f, 0.5f, 1, 1, 1,
        
        // Back face
        -0.5f, -0.5f, -0.5f, 1, 1, 1,
        0.5f, -0.5f, -0.5f, 1, 1, 1,
        0.5f, 0.5f, -0.5f, 1, 1, 1,
        -0.5f, 0.5f, -0.5f, 1, 1, 1,
        
        // Left face
        -0.5f, -0.5f, 0.5f, 1, 1, 1,
        -0.5f, -0.5f, -0.5f, 1, 1, 1,
        -0.5f, 0.5f, -0.5f, 1, 1, 1,
        -0.5f, 0.5f, 0.5f, 1, 1, 1,
        
        // Right face
        0.5f, -0.5f, 0.5f, 1, 1, 1,
        0.5f, -0.5f, -0.5f, 1, 1, 1,
        0.5f, 0.5f, -0.5f, 1, 1, 1,
        0.5f, 0.5f, 0.5f, 1, 1, 1,
        
        // Top face
        -0.5f, 0.5f, 0.5f, 1, 1, 1,
        0.5f, 0.5f, 0.5f, 1, 1, 1,
        0.5f, 0.5f, -0.5f, 1, 1, 1,
        -0.5f, 0.5f, -0.5f, 1, 1, 1,
        
        // Bottom face
        -0.5f, -0.5f, 0.5f, 1, 1, 1,
        0.5f, -0.5f, 0.5f, 1, 1, 1,
        0.5f, -0.5f, -0.5f, 1, 1, 1,
        -0.5f, -0.5f, -0.5f, 1, 1, 1
    };
    
    protected override bool UseVertexColors => false;
    protected override Matrix4 ModelMatrix { get; }
    
    
    public DebugBox(Vector3 position, Vector3 size, Color4 color, float lifetimeSeconds)
    {
        Position = position;
        Size = size;
        Color = color;
        LifetimeSeconds = lifetimeSeconds;
        ModelMatrix = Matrix4.CreateScale(Size) * Matrix4.CreateTranslation(Position);
    }


    protected override void Draw()
    {
        GL.BindVertexArray(DebugDrawer.VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, DebugDrawer.VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);
        GL.DrawArrays(PrimitiveType.LineLoop, 4, 4);
        GL.DrawArrays(PrimitiveType.LineLoop, 8, 4);
        GL.DrawArrays(PrimitiveType.LineLoop, 12, 4);
        GL.DrawArrays(PrimitiveType.LineLoop, 16, 4);
        GL.DrawArrays(PrimitiveType.LineLoop, 20, 4);
    }
}


public class DebugSphere : DebugDrawable
{
    private Vector3 Position { get; }
    private float Radius { get; }
    private static readonly float[] Vertices;
    
    protected override bool UseVertexColors => false;
    protected override Matrix4 ModelMatrix { get; }


    static DebugSphere()
    {
        List<float> vertices = new();
        const int precision = 20; // Increase for a more detailed sphere.
        for (int i = 0; i <= precision; i++)
        {
            double lat = Math.PI * i / precision;
            for (int j = 0; j <= precision; j++)
            {
                double lon = 2 * Math.PI * j / precision;
                float x = (float)(Math.Cos(lon) * Math.Sin(lat));
                float y = (float)Math.Cos(lat);
                float z = (float)(Math.Sin(lon) * Math.Sin(lat));
                vertices.AddRange(new[] { 0.5f * x, 0.5f * y, 0.5f * z, 1, 1, 1 });
            }
        }
        Vertices = vertices.ToArray();
    }
    

    public DebugSphere(Vector3 position, float radius, Color4 color, float lifetimeSeconds)
    {
        Position = position;
        Radius = radius;
        Color = color;
        LifetimeSeconds = lifetimeSeconds;
        ModelMatrix = Matrix4.CreateScale(new Vector3(Radius * 2, Radius * 2, Radius * 2)) * Matrix4.CreateTranslation(Position);
    }


    protected override void Draw()
    {
        GL.BindVertexArray(DebugDrawer.VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, DebugDrawer.VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.DrawArrays(PrimitiveType.LineLoop, 0, Vertices.Length / 6);
    }
}


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
        ShaderManager.DebugShader.Use();

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

            drawable.LifetimeSeconds -= (float)Time.DeltaTime;
        }

        foreach (DebugDrawable drawable in drawablesToRemove)
            TemporaryDrawables.Remove(drawable);
    }
}