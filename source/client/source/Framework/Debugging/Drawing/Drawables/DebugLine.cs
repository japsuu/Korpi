using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Debugging.Drawing.Drawables;

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