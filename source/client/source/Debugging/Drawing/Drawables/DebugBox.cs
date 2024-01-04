using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Debugging.Drawing.Drawables;

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