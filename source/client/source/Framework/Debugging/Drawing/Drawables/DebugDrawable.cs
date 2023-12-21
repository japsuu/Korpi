using BlockEngine.Client.Framework.Rendering.Shaders;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Debugging.Drawing.Drawables;

public abstract class DebugDrawable
{
    protected abstract bool UseVertexColors { get; }
    protected abstract Matrix4 ModelMatrix { get; }
    
    protected Color4 Color;
    
    public float LifetimeSeconds;
    
    
    public void DrawObject()
    {
        ShaderManager.DebugShader.SetMatrix4("model", ModelMatrix);
        if (UseVertexColors)
        {
            ShaderManager.DebugShader.SetVector3("overrideColor", new Vector3(1, 1, 1));
            Draw();
        }
        else
        {
            ShaderManager.DebugShader.SetVector3("overrideColor", new Vector3(Color.R, Color.G, Color.B));
            Draw();
        }
    }


    protected abstract void Draw();
}