using Korpi.Client.Rendering.Shaders.Sources;
using Korpi.Client.Rendering.Shaders.Variables;
using Korpi.Client.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

[VertexShaderSource("core/blocks_common.vert")]
[FragmentShaderSource("core/blocks_opaque_cutout.frag")]
public class BlocksOpaqueCutoutShaderProgram : MvpShaderProgram
{
    /// <summary>
    /// Two unsigned integers of data per vertex.
    /// </summary>
    [VertexAttrib(2, VertexAttribPointerType.UnsignedInt)]
    public VertexAttrib InData { get; protected set; } = null!;
    
    /// <summary>
    /// The color modulator is used to tint the texture.
    /// Can be used for effects like global illumination.
    /// </summary>
    public Uniform<Vector3> ColorModulator { get; protected set; } = null!;
    
    /// <summary>
    /// Block array texture.
    /// </summary>
    public TextureUniform<Texture2DArray> Sampler0 { get; protected set; } = null!;
}