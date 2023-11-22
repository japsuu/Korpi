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
    
    public static float WindowWidth { get; private set; }
    public static float WindowHeight { get; private set; }


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
    
    
    public static void UpdateWindowSize(float width, float height)
    {
        WindowWidth = width;
        WindowHeight = height;
    }
    
    
    /// <returns>If the provided world position is visible on screen.</returns>
    public static bool WorldPositionToScreenPosition(Vector3 worldPosition, out Vector2 screenPos)
    {
        Vector4 clipSpacePosition = new Vector4(worldPosition, 1) * ViewMatrix * ProjectionMatrix;
        
        // Without this the coordinates are visible even when looking straight away from them.
        if (clipSpacePosition.W <= 0)
        {
            screenPos = Vector2.NegativeInfinity;
            return false;
        }
        
        Vector3 normalizedDeviceCoordinates = clipSpacePosition.Xyz / clipSpacePosition.W;
        Vector2 screenCoordinates = new Vector2(normalizedDeviceCoordinates.X, -normalizedDeviceCoordinates.Y);
        screenCoordinates += Vector2.One;
        screenCoordinates /= 2;
        screenCoordinates.X *= WindowWidth;
        screenCoordinates.Y *= WindowHeight;
        screenPos = screenCoordinates;
        return true;
    }
    
    
    public void Dispose()
    {
        DebugShader.Dispose();
        ChunkShader.Dispose();
        SkyboxShader.Dispose();
        
        GC.SuppressFinalize(this);
    }
}