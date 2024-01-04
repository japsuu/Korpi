using BlockEngine.Client.Rendering.Shaders;
using BlockEngine.Client.Rendering.Textures;
using BlockEngine.Client.Utils;
using BlockEngine.Client.Window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Rendering.Skybox;

/// <summary>
/// Three-dimensional cubic sun, that moves across the skybox.
/// Rotates around the X and Y axes.
/// </summary>
public class Sun : SkyboxFeature, IDisposable
{
    private readonly float[] _vertices =
    {
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        -1.0f,  1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f,  1.0f
    };
    
    private readonly CubemapTexture _texture;
    private readonly int _vao;
    private readonly bool _enableRotation;
    
    
    public Sun(bool enableRotation)
    {
        _texture = CubemapTexture.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxSunTexturePath("sun.png"),
            IoUtils.GetSkyboxSunTexturePath("sun.png"),
            IoUtils.GetSkyboxSunTexturePath("sun.png"),
            IoUtils.GetSkyboxSunTexturePath("sun.png"),
            IoUtils.GetSkyboxSunTexturePath("sun.png"),
            IoUtils.GetSkyboxSunTexturePath("sun.png"),
        }, "Skybox Decor (sun)");
        
        
        _enableRotation = enableRotation;
        
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        
        int vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        
        ShaderManager.SunShader.Use();
        _texture.BindStatic(TextureUnit.Texture4);
        ShaderManager.SunShader.SetInt("cubeTexture", 4);
    }
    
    
    public void Render()
    {
        ShaderManager.SunShader.Use();

        Vector3 position = GameTime.SunDirection * 800;
        
        Matrix4 modelMatrix = Matrix4.Identity;
        modelMatrix *= Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(1f * GameTime.TotalTime));
        modelMatrix *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(0.01f * GameTime.TotalTime));
        modelMatrix *= Matrix4.CreateScale(10f);
        modelMatrix *= Matrix4.CreateTranslation(position.X, position.Y, position.Z);
        
        Matrix4 skyboxViewMatrix = new(new Matrix3(ShaderManager.ViewMatrix)); // Remove translation from the view matrix
        ShaderManager.SunShader.SetMatrix4("model", modelMatrix);
        ShaderManager.SunShader.SetMatrix4("view", skyboxViewMatrix);
        
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
    }


    private void ReleaseUnmanagedResources()
    {
        _texture.Dispose();
        GL.DeleteVertexArray(_vao);
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~Sun()
    {
        ReleaseUnmanagedResources();
    }
}