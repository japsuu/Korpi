using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Rendering.Shaders;

public class ShaderManager : IDisposable
{
    public static Shader DebugShader { get; private set; } = null!;
    public static Shader ChunkShader { get; private set; } = null!;
    public static Shader SkyboxShader { get; private set; } = null!;
    
    public static Matrix4 ProjectionMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewMatrix { get; private set; } = Matrix4.Identity;


    public ShaderManager()
    {
        DebugShader = new Shader(IoUtils.GetShaderPath("shader_blocks.vert"), IoUtils.GetShaderPath("shader_blocks.frag"));
        DebugShader.Use();
        
        ChunkShader = new Shader(IoUtils.GetShaderPath("shader_chunk.vert"), IoUtils.GetShaderPath("shader_chunk.frag"));
        ChunkShader.Use();
        
        SkyboxShader = new Shader(IoUtils.GetShaderPath("shader_skybox.vert"), IoUtils.GetShaderPath("shader_skybox.frag"));
        SkyboxShader.Use();
    }
    
    
    public static void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        ProjectionMatrix = projectionMatrix;
        
        DebugShader.Use();
        DebugShader.SetMatrix4("projection", projectionMatrix);
        
        ChunkShader.Use();
        ChunkShader.SetMatrix4("projection", projectionMatrix);
        
        SkyboxShader.Use();
        SkyboxShader.SetMatrix4("projection", projectionMatrix);
    }
    
    
    public static void UpdateViewMatrix(Matrix4 viewMatrix, Matrix4 skyboxViewMatrix)
    {
        ViewMatrix = viewMatrix;
        
        DebugShader.Use();
        DebugShader.SetMatrix4("view", viewMatrix);
        
        ChunkShader.Use();
        ChunkShader.SetMatrix4("view", viewMatrix);
        
        SkyboxShader.Use();
        SkyboxShader.SetMatrix4("view", skyboxViewMatrix);
    }
    
    
    public void Dispose()
    {
        DebugShader.Dispose();
        ChunkShader.Dispose();
        SkyboxShader.Dispose();
        
        GC.SuppressFinalize(this);
    }
}