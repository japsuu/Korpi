using BlockEngine.Client.Framework.Rendering.Shaders;
using BlockEngine.Client.Framework.Rendering.Textures;
using BlockEngine.Client.Utils;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Client.Framework.Rendering;

public class Crosshair
{
    private static readonly float[] Vertices = 
    {
        // Position     // Texture coords
        -0.05f,  0.05f,  0.0f, 0.0f,
         0.05f, -0.05f,  1.0f, 1.0f,
        -0.05f, -0.05f,  0.0f, 1.0f,
        
        -0.05f,  0.05f,  0.0f, 0.0f,
         0.05f, -0.05f,  1.0f, 1.0f,
         0.05f,  0.05f,  1.0f, 0.0f 
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
        _crosshairTexture.BindStatic(TextureUnit.Texture2);
        
        GameClient.ClientUnload += OnClientUnload;
    }


    public void Draw()
    {
        _shader.Use();
        _shader.SetFloat("aspectRatio", ShaderManager.WindowWidth / (float)ShaderManager.WindowHeight);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
    }
    
    
    private void OnClientUnload()
    {
        _crosshairTexture.Dispose();
        _shader.Dispose();
        
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
    }
}