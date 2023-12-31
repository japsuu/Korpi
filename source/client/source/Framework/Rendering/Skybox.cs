using BlockEngine.Client.Framework.Rendering.Shaders;
using BlockEngine.Client.Framework.Rendering.Textures;
using BlockEngine.Client.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Rendering;

public class Skybox : IDisposable
{
    private readonly float[] _skyboxVertices = {
        // Z- face
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        // X- face
        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        // X+ face
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,

        // Z+ face
        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        // Y+ face
        -1.0f,  1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        // Y- face
        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f,  1.0f
    };
    
    private readonly Sun _sun;
    private readonly CubemapTexture _daySkyboxTexture;
    private readonly CubemapTexture _nightSkyboxTexture;
    private readonly int _skyboxVAO;
    private readonly bool _enableStarsRotation;


    public Skybox(bool enableStarsRotation)
    {
        _enableStarsRotation = enableStarsRotation;
        
        // Generate the VAO and VBO.
        _skyboxVAO = GL.GenVertexArray();
        int skyboxVBO = GL.GenBuffer();
        GL.BindVertexArray(_skyboxVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _skyboxVertices.Length * sizeof(float), _skyboxVertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        // Load the skybox textures.
        _daySkyboxTexture = CubemapTexture.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxTexturePath("day_x_neg.png"),
            IoUtils.GetSkyboxTexturePath("day_x_pos.png"),
            IoUtils.GetSkyboxTexturePath("day_y_neg.png"),
            IoUtils.GetSkyboxTexturePath("day_y_pos.png"),
            IoUtils.GetSkyboxTexturePath("day_z_neg.png"),
            IoUtils.GetSkyboxTexturePath("day_z_pos.png"),
        }, "Skybox Day");
        
        // Load the skybox textures.
        _nightSkyboxTexture = CubemapTexture.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxTexturePath("night_x_neg.png"),
            IoUtils.GetSkyboxTexturePath("night_x_pos.png"),
            IoUtils.GetSkyboxTexturePath("night_y_neg.png"),
            IoUtils.GetSkyboxTexturePath("night_y_pos.png"),
            IoUtils.GetSkyboxTexturePath("night_z_neg.png"),
            IoUtils.GetSkyboxTexturePath("night_z_pos.png"),
        }, "Skybox Night");
        
        _daySkyboxTexture.BindStatic(TextureUnit.Texture1);
        _nightSkyboxTexture.BindStatic(TextureUnit.Texture2);
        
        ShaderManager.SkyboxShader.Use();
        ShaderManager.SkyboxShader.SetInt("skyboxDayTexture", 1);
        ShaderManager.SkyboxShader.SetInt("skyboxNightTexture", 2);
        
        _sun = new Sun(true);
    }


    public void Draw()
    {
        // Update the skybox view matrix.
        Matrix4 skyboxViewMatrix = new(new Matrix3(ShaderManager.ViewMatrix)); // Remove translation from the view matrix
        Matrix4 modelMatrix = Matrix4.Identity;
        if (_enableStarsRotation)
        {
            modelMatrix *= Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_X * Time.TotalTime)) *
                           Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_Y * Time.TotalTime));
            skyboxViewMatrix = modelMatrix * skyboxViewMatrix;
        }
        ShaderManager.SkyboxShader.Use();
        ShaderManager.SkyboxShader.SetMatrix4("model", modelMatrix);
        ShaderManager.SkyboxShader.SetMatrix4("view", skyboxViewMatrix);
        ShaderManager.SkyboxShader.SetVector3("sunDirection", GameTime.SunDirection);
        ShaderManager.SkyboxShader.SetFloat("skyboxLerpProgress", GameTime.SkyboxLerpProgress);
        
        // Draw the skybox.
        GL.DepthFunc(DepthFunction.Lequal);  // Change depth function so depth test passes when values are equal to depth buffer's content
        
        GL.BindVertexArray(_skyboxVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
        
        _sun.Render();
        
        GL.DepthFunc(DepthFunction.Less); // set depth function back to default
    }


    private void ReleaseUnmanagedResources()
    {
        _daySkyboxTexture.Dispose();
        _nightSkyboxTexture.Dispose();
        GL.DeleteVertexArray(_skyboxVAO);
        _sun.Dispose();
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~Skybox()
    {
        ReleaseUnmanagedResources();
    }
}