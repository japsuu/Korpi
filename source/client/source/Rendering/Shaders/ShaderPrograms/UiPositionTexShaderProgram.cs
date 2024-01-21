using Korpi.Client.Rendering.Shaders.Sources;
using Korpi.Client.Rendering.Shaders.Variables;
using Korpi.Client.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

[VertexShaderSource("core/ui_position_tex.vert")]
[FragmentShaderSource("core/ui_position_tex.frag")]
public class UiPositionTexShaderProgram : ShaderProgram
{
    [VertexAttrib(3, VertexAttribPointerType.Float)]
    public VertexAttrib InPosition { get; protected set; } = null!;
    
    [VertexAttrib(2, VertexAttribPointerType.Float)]
    public VertexAttrib UV0 { get; protected set; } = null!;
    
    public TextureUniform<Texture2D> Sampler0 { get; protected set; } = null!;
    //public Uniform<Vector4> ColorModulator { get; protected set; } = null!;
}