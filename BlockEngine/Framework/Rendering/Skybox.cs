using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Framework.Rendering.Textures;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;

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


    public Skybox()
    {
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
        GL.DepthFunc(DepthFunction.Lequal);  // Change depth function so depth test passes when values are equal to depth buffer's content
        
        ShaderManager.SkyboxShader.Use();
        
        _skyboxTexture.Use(TextureUnit.Texture0);
        GL.BindVertexArray(_skyboxVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
        
        GL.DepthFunc(DepthFunction.Less); // set depth function back to default
    }
}