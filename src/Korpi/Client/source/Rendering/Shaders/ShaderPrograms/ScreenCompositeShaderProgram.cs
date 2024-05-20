using KorpiEngine.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Rendering.Shaders.Sources;
using KorpiEngine.Rendering.Shaders.Variables;
using KorpiEngine.Rendering.Textures;
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