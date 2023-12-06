using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Debugging;

public abstract class DebugDrawable
{
    public Color4 Color;
    public float LifetimeSeconds;
    
    public abstract void Draw();
}


public class DebugLine : DebugDrawable
{
    private Vector3 Start { get; }
    private Vector3 End { get; }
    private float[] Vertices { get; }

    public DebugLine(Vector3 start, Vector3 end, Color4 color, float lifetimeSeconds)
    {
        Start = start;
        End = end;
        Color = color;
        LifetimeSeconds = lifetimeSeconds;

        Vertices = new[]
        {
            Start.X, Start.Y, Start.Z, Color.R, Color.G, Color.B,
            End.X, End.Y, End.Z, Color.R, Color.G, Color.B
        };
    }

    public override void Draw()
    {
        Matrix4 modelMatrix = Matrix4.Identity;
        ShaderManager.DebugShader.SetMatrix4("model", modelMatrix);

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
    private float[] Vertices { get; }


    public DebugBox(Vector3 position, Vector3 size, Color4 color, float lifetimeSeconds)
    {
        Position = position;
        Size = size;
        Color = color;
        LifetimeSeconds = lifetimeSeconds;
        
        float sizeX = Size.X / 2f;
        float sizeY = Size.Y / 2f;
        float sizeZ = Size.Z / 2f;

        Vertices = new[]
        {
            // Front face
            Position.X - sizeX, Position.Y - sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y - sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y + sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y + sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            
            // Back face
            Position.X - sizeX, Position.Y - sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y - sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y + sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y + sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            
            // Left face
            Position.X - sizeX, Position.Y - sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y - sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y + sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y + sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            
            // Right face
            Position.X + sizeX, Position.Y - sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y - sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y + sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y + sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            
            // Top face
            Position.X - sizeX, Position.Y + sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y + sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y + sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y + sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            
            // Bottom face
            Position.X - sizeX, Position.Y - sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y - sizeY, Position.Z + sizeZ, Color.R, Color.G, Color.B,
            Position.X + sizeX, Position.Y - sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B,
            Position.X - sizeX, Position.Y - sizeY, Position.Z - sizeZ, Color.R, Color.G, Color.B
        };
    }


    public override void Draw()
    {
        Matrix4 modelMatrix = Matrix4.Identity;
        ShaderManager.DebugShader.SetMatrix4("model", modelMatrix);

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
    private float[] Vertices { get; }

    public DebugSphere(Vector3 position, float radius, Color4 color, float lifetimeSeconds)
    {
        Position = position;
        Radius = radius;
        Color = color;
        LifetimeSeconds = lifetimeSeconds;

        // Generate vertices for the sphere
        List<float> vertices = new();
        const int precision = 20; // Increase for a more detailed sphere    TODO: Instead of generating the vertices for each sphere, use the model matrix.
        for (int i = 0; i <= precision; i++)
        {
            double lat = Math.PI * i / precision;
            for (int j = 0; j <= precision; j++)
            {
                double lon = 2 * Math.PI * j / precision;
                float x = (float)(Math.Cos(lon) * Math.Sin(lat));
                float y = (float)Math.Cos(lat);
                float z = (float)(Math.Sin(lon) * Math.Sin(lat));
                vertices.AddRange(new[] { Position.X + Radius * x, Position.Y + Radius * y, Position.Z + Radius * z, Color.R, Color.G, Color.B });
            }
        }
        Vertices = vertices.ToArray();
    }

    public override void Draw()
    {
        Matrix4 modelMatrix = Matrix4.Identity;
        ShaderManager.DebugShader.SetMatrix4("model", modelMatrix);

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
            drawable.Draw();

        Drawables.Clear();

        List<DebugDrawable> drawablesToRemove = new();
        foreach (DebugDrawable drawable in TemporaryDrawables)
        {
            if (drawable.LifetimeSeconds <= 0f)
                drawablesToRemove.Add(drawable);
            else
                drawable.Draw();

            drawable.LifetimeSeconds -= (float)Time.DeltaTime;
        }

        foreach (DebugDrawable drawable in drawablesToRemove)
            TemporaryDrawables.Remove(drawable);
    }
}