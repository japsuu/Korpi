using Korpi.Client.Rendering.Shaders.Sources;
using Korpi.Client.Rendering.Shaders.Variables;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

[VertexShaderSource("core/position_color.vert")]
[FragmentShaderSource("core/position_color.frag")]
public class PositionColorShaderProgram : MvpShaderProgram
{
    [VertexAttrib(3, VertexAttribPointerType.Float)]
    public VertexAttrib InPosition { get; protected set; } = null!;
    
    [VertexAttrib(4, VertexAttribPointerType.Float)]
    public VertexAttrib InColor { get; protected set; } = null!;
    
    public Uniform<Vector4> ColorModulator { get; protected set; } = null!;
}