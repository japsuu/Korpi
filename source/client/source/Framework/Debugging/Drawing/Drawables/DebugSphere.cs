using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Debugging.Drawing.Drawables;

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