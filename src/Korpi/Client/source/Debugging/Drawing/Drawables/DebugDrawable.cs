using Korpi.Client.Rendering.Shaders;
using OpenTK.Mathematics;

namespace Korpi.Client.Debugging.Drawing.Drawables;

public abstract class DebugDrawable
{
    protected abstract bool UseVertexColors { get; }
    protected abstract Matrix4 ModelMatrix { get; }
    
    protected Color4 Color;
    
    public float LifetimeSeconds;
    
    
    public void DrawObject()
    {
        ShaderManager.PositionColorShader.ModelMat.Set(ModelMatrix);
        if (UseVertexColors)
        {
            ShaderManager.PositionColorShader.ColorModulator.Set(new Vector4(1, 1, 1, 1));
            Draw();
        }
        else
        {
            ShaderManager.PositionColorShader.ColorModulator.Set(new Vector4(Color.R, Color.G, Color.B, Color.A));
            Draw();
        }
    }


    protected abstract void Draw();
}