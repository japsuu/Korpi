using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Framework.Rendering.Textures;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Rendering;

public class Skybox
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
    
    private readonly CubemapTexture _skyboxTexture;
    private readonly int _skyboxVAO;
    private readonly bool _enableRotation;


    public Skybox(bool enableRotation)
    {
        _enableRotation = enableRotation;
        
        // Generate the VAO and VBO.
        _skyboxVAO = GL.GenVertexArray();
        int skyboxVBO = GL.GenBuffer();
        GL.BindVertexArray(_skyboxVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _skyboxVertices.Length * sizeof(float), _skyboxVertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        // Load the skybox texture.
        _skyboxTexture = CubemapTexture.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxTexturePath("x_neg.png"),
            IoUtils.GetSkyboxTexturePath("x_pos.png"),
            IoUtils.GetSkyboxTexturePath("y_neg.png"),
            IoUtils.GetSkyboxTexturePath("y_pos.png"),
            IoUtils.GetSkyboxTexturePath("z_pos.png"),
            IoUtils.GetSkyboxTexturePath("z_neg.png"),
        });
    }


    public void Draw()
    {
        // Update the skybox view matrix.
        Matrix4 skyboxViewMatrix = new(new Matrix3(ShaderManager.ViewMatrix)); // Remove translation from the view matrix
        if (_enableRotation)
        {
            Matrix4 modelMatrix = Matrix4.Identity *
                                  Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_X * Time.TotalTime)) *
                                  Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_Y * Time.TotalTime));
            skyboxViewMatrix = modelMatrix * skyboxViewMatrix;
        }
        ShaderManager.UpdateSkyboxViewMatrix(skyboxViewMatrix);
        
        // Draw the skybox.
        GL.DepthFunc(DepthFunction.Lequal);  // Change depth function so depth test passes when values are equal to depth buffer's content
        
        ShaderManager.SkyboxShader.Use();
        
        _skyboxTexture.Use(TextureUnit.Texture0);
        GL.BindVertexArray(_skyboxVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
        
        GL.DepthFunc(DepthFunction.Less); // set depth function back to default
    }
}