using KorpiEngine.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Rendering.Shaders.Sources;
using KorpiEngine.Rendering.Shaders.Variables;
using KorpiEngine.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

[VertexShaderSource("core/skybox.vert")]
[FragmentShaderSource("core/skybox.frag")]
public class SkyboxShaderProgram : MVPShaderProgram
{
    [VertexAttrib(3, VertexAttribPointerType.Float)]
    public VertexAttrib InPosition { get; protected set; } = null!;
    
    public Uniform<Vector3> SunDirection { get; protected set; } = null!;
    public Uniform<float> SkyboxLerpProgress { get; protected set; } = null!;
    
    public TextureUniform<TextureCubemap> DayTexture { get; protected set; } = null!;
    public TextureUniform<TextureCubemap> NightTexture { get; protected set; } = null!;


    protected override void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        Use();
        ViewMat.Set(viewMatrix.ClearTranslation());
    }
}