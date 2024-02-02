using Korpi.Client.Configuration;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Rendering.Textures;
using Korpi.Client.Utils;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.UI.HUD;

public sealed class Crosshair : IDisposable
{
    private static readonly float[] Vertices =
    {
        // Position              // Texture coords
        -0.025f,  0.025f, 0.0f,  0.0f, 0.0f, // Top-left corner
        -0.025f, -0.025f, 0.0f,  0.0f, 1.0f, // Bottom-left corner
        0.025f, -0.025f, 0.0f,  1.0f, 1.0f, // Bottom-right corner

        0.025f, -0.025f, 0.0f,  1.0f, 1.0f, // Bottom-right corner
        0.025f,  0.025f, 0.0f,  1.0f, 0.0f, // Top-right corner
        -0.025f,  0.025f, 0.0f,  0.0f, 0.0f  // Top-left corner
    };
    
    private readonly Texture2D _crosshairTexture;
    private readonly int _vao;
    private readonly int _vbo;
    
    
    public Crosshair()
    {
        _crosshairTexture = Texture2D.LoadFromFile(IoUtils.GetTexturePath("Crosshair.png"), "Crosshair");
        
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
        
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
    }


    public void Draw()
    {
#if DEBUG
        if (!ClientConfig.Rendering.Debug.RenderCrosshair)
            return;
#endif
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        ShaderManager.UiPositionTexShader.Use();
        _crosshairTexture.Bind(TextureUnit.Texture0);
        
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
        
        GL.Disable(EnableCap.Blend);
    }
    
    
    public void Dispose()
    {
        _crosshairTexture.Dispose();
        
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
    }
}