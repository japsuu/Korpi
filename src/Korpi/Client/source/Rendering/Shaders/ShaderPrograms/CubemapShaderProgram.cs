using Korpi.Client.Rendering.Shaders.Sources;
using Korpi.Client.Rendering.Shaders.Variables;
using Korpi.Client.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

/// <summary>
/// Shader for rendering a cubemap texture.
/// View matrix has translation component removed, so that camera movement does not affect the position of the mesh.
/// <see cref="CubemapShaderProgram.InPosition"/> (vertex pos) is used to sample the cubemap.
/// </summary>
[VertexShaderSource("core/cubemap_tex.vert")]
[FragmentShaderSource("core/cubemap_tex.frag")]
public class CubemapShaderProgram : MvpShaderProgram
{
    [VertexAttrib(3, VertexAttribPointerType.Float)]
    public VertexAttrib InPosition { get; protected set; } = null!;
    
    public TextureUniform<Texture2D> Sampler0 { get; protected set; } = null!;


    protected override void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        Use();
        ViewMat.Set(viewMatrix.ClearTranslation());
    }
}