using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Rendering.Shaders.Sources;
using KorpiEngine.Core.Rendering.Shaders.Variables;
using KorpiEngine.Core.Rendering.Textures;
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