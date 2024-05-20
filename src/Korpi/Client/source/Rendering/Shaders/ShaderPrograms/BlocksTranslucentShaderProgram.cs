using KorpiEngine.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Rendering.Shaders.Sources;
using KorpiEngine.Rendering.Shaders.Variables;
using KorpiEngine.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

[VertexShaderSource("core/blocks_common.vert")]
[FragmentShaderSource("core/blocks_translucent.frag")]
public class BlocksTranslucentShaderProgram : MVPShaderProgram
{
    /// <summary>
    /// Two unsigned integers of data per vertex.
    /// </summary>
    [VertexAttrib(2, VertexAttribPointerType.UnsignedInt)]
    public VertexAttrib InData { get; protected set; } = null!;

    // /// <summary>
    // /// The color modulator is used to tint the texture.
    // /// Can be used for effects like global illumination.
    // /// </summary>
    public Uniform<Vector3> ColorModulator { get; protected set; } = null!;
    
    /// <summary>
    /// Block array texture.
    /// </summary>
    public TextureUniform<Texture2DArray> Sampler0 { get; protected set; } = null!;
}