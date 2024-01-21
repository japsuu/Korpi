using Korpi.Client.Rendering.Shaders.Sources;
using Korpi.Client.Rendering.Shaders.Variables;
using Korpi.Client.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

[VertexShaderSource("core/ui_position.vert")]
[FragmentShaderSource("core/screen_composite.frag")]
public class ScreenCompositeShaderProgram : ShaderProgram
{
    [VertexAttrib(3, VertexAttribPointerType.Float)]
    public VertexAttrib InPosition { get; protected set; } = null!;
    
    public TextureUniform<Texture2D> AccumulationTexture { get; protected set; } = null!;
    public TextureUniform<Texture2D> RevealageTexture { get; protected set; } = null!;
}