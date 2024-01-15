using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Rendering.Textures;
using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.UI.HUD;

public class Crosshair
{
    private static readonly float[] Vertices = 
    {
        // Position     // Texture coords
        -0.025f,  0.025f,  0.0f, 0.0f,
         0.025f, -0.025f,  1.0f, 1.0f,
        -0.025f, -0.025f,  0.0f, 1.0f,
        
        -0.025f,  0.025f,  0.0f, 0.0f,
         0.025f, -0.025f,  1.0f, 1.0f,
         0.025f,  0.025f,  1.0f, 0.0f 
    };
    
    private readonly Texture2D _crosshairTexture;
    private readonly Shader _shader;
    private readonly int _vao;
    private readonly int _vbo;
    
    
    public Crosshair()
    {
        _crosshairTexture = Texture2D.LoadFromFile(IoUtils.GetTexturePath("Crosshair.png"), "Crosshair");
        _shader = new Shader(IoUtils.GetShaderPath("shader_crosshair.vert"), IoUtils.GetShaderPath("shader_crosshair.frag"));
        
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
        
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        
        _shader.Use();
        _crosshairTexture.BindStatic(TextureUnit.Texture3);
        
        GameClient.ClientUnload += OnClientUnload;
    }


    public void Draw()
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _shader.Use();
        _shader.SetFloat("aspectRatio", GameClient.WindowAspectRatio);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
        GL.Disable(EnableCap.Blend);
    }
    
    
    private void OnClientUnload()
    {
        _crosshairTexture.Dispose();
        _shader.Dispose();
        
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
    }
}