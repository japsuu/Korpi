using Korpi.Client.Rendering.Shaders.Variables;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders.ShaderPrograms;

public class MvpShaderProgram : ShaderProgram
{
    public Uniform<Matrix4> ModelMat { get; protected set; } = null!;
    public Uniform<Matrix4> ViewMat { get; protected set; } = null!;
    public Uniform<Matrix4> ProjMat { get; protected set; } = null!;


    protected MvpShaderProgram()
    {
        ShaderManager.ProjectionMatrixChanged += UpdateProjectionMatrix;
        ShaderManager.ViewMatrixChanged += UpdateViewMatrix;
    }
    
    
    protected virtual void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        Use();
        ProjMat.Set(projectionMatrix);
    }


    protected virtual void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        Use();
        ViewMat.Set(viewMatrix);
    }


    protected override void Dispose(bool disposing)
    {
        ShaderManager.ProjectionMatrixChanged -= UpdateProjectionMatrix;
        ShaderManager.ViewMatrixChanged -= UpdateViewMatrix;
        
        base.Dispose(disposing);
    }
}