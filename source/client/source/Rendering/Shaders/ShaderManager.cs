using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders;

public class ShaderManager : IDisposable
{
    public static Shader PassShader { get; private set; } = null!;
    public static Shader DebugShader { get; private set; } = null!;
    public static Shader OpaqueChunkShader { get; private set; } = null!;
    public static Shader TransparentChunkShader { get; private set; } = null!;
    public static Shader SkyboxShader { get; private set; } = null!;
    public static Shader CelestialBodyShader { get; private set; } = null!;
    
    public static Matrix4 ProjectionMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewMatrix { get; private set; } = Matrix4.Identity;


    public ShaderManager()
    {
        PassShader = new Shader(IoUtils.GetShaderPath("shader_pass.vert"), IoUtils.GetShaderPath("shader_pass.frag"));
        PassShader.Use();
        
        DebugShader = new Shader(IoUtils.GetShaderPath("shader_debug.vert"), IoUtils.GetShaderPath("shader_debug.frag"));
        DebugShader.Use();
        
        OpaqueChunkShader = new Shader(IoUtils.GetShaderPath("shader_chunk_opaque.vert"), IoUtils.GetShaderPath("shader_chunk_opaque.frag"));
        OpaqueChunkShader.Use();
        
        TransparentChunkShader = new Shader(IoUtils.GetShaderPath("shader_chunk_transparent.vert"), IoUtils.GetShaderPath("shader_chunk_transparent.frag"));
        TransparentChunkShader.Use();
        
        SkyboxShader = new Shader(IoUtils.GetShaderPath("shader_skybox.vert"), IoUtils.GetShaderPath("shader_skybox.frag"));
        SkyboxShader.Use();
        
        CelestialBodyShader = new Shader(IoUtils.GetShaderPath("shader_celestial_body.vert"), IoUtils.GetShaderPath("shader_celestial_body.frag"));
        CelestialBodyShader.Use();
    }
    
    
    public static void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        ProjectionMatrix = projectionMatrix;
        
        PassShader.Use();
        PassShader.SetMatrix4("projection", projectionMatrix);
        
        DebugShader.Use();
        DebugShader.SetMatrix4("projection", projectionMatrix);
        
        OpaqueChunkShader.Use();
        OpaqueChunkShader.SetMatrix4("projection", projectionMatrix);
        
        TransparentChunkShader.Use();
        TransparentChunkShader.SetMatrix4("projection", projectionMatrix);
        
        SkyboxShader.Use();
        SkyboxShader.SetMatrix4("projection", projectionMatrix);
        
        CelestialBodyShader.Use();
        CelestialBodyShader.SetMatrix4("projection", projectionMatrix);
    }
    
    
    public static void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        ViewMatrix = viewMatrix;
        
        PassShader.Use();
        PassShader.SetMatrix4("view", viewMatrix);
        
        DebugShader.Use();
        DebugShader.SetMatrix4("view", viewMatrix);
        
        OpaqueChunkShader.Use();
        OpaqueChunkShader.SetMatrix4("view", viewMatrix);
        
        TransparentChunkShader.Use();
        TransparentChunkShader.SetMatrix4("view", viewMatrix);
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
        screenCoordinates.X *= GameClient.WindowWidth;
        screenCoordinates.Y *= GameClient.WindowHeight;
        screenPos = screenCoordinates;
        return true;
    }
    
    
    public void Dispose()
    {
        DebugShader.Dispose();
        OpaqueChunkShader.Dispose();
        TransparentChunkShader.Dispose();
        SkyboxShader.Dispose();
        CelestialBodyShader.Dispose();
        
        GC.SuppressFinalize(this);
    }
}