using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders;

public class ShaderManager : IDisposable
{
    public static Shader ShaderPlayer { get; private set; } = null!;
    public static Shader DebugShader { get; private set; } = null!;
    public static Shader ShaderBlockOpaque { get; private set; } = null!;
    public static Shader ShaderBlockCutout { get; private set; } = null!;
    public static Shader ShaderBlockTranslucent { get; private set; } = null!;
    public static Shader ShaderComposite { get; private set; } = null!;
    public static Shader ShaderScreen { get; private set; } = null!;
    public static Shader ShaderUi { get; private set; } = null!;
    public static Shader SkyboxShader { get; private set; } = null!;
    public static Shader CelestialBodyShader { get; private set; } = null!;
    
    public static Matrix4 ProjectionMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewMatrix { get; private set; } = Matrix4.Identity;


    public ShaderManager()
    {
        ShaderPlayer = new Shader(IoUtils.GetShaderPath("shader_player.vert"), IoUtils.GetShaderPath("shader_player.frag"));
        DebugShader = new Shader(IoUtils.GetShaderPath("shader_debug.vert"), IoUtils.GetShaderPath("shader_debug.frag"));
        ShaderBlockOpaque = new Shader(IoUtils.GetShaderPath("blocks_common.vert"), IoUtils.GetShaderPath("blocks_opaque.frag"));
        ShaderBlockCutout = new Shader(IoUtils.GetShaderPath("blocks_common.vert"), IoUtils.GetShaderPath("blocks_cutout.frag"));
        ShaderBlockTranslucent = new Shader(IoUtils.GetShaderPath("blocks_common.vert"), IoUtils.GetShaderPath("blocks_translucent.frag"));
        ShaderComposite = new Shader(IoUtils.GetShaderPath("pass.vert"), IoUtils.GetShaderPath("composite.frag"));
        ShaderScreen = new Shader(IoUtils.GetShaderPath("screen.vert"), IoUtils.GetShaderPath("screen.frag"));
        ShaderUi = new Shader(IoUtils.GetShaderPath("shader_ui.vert"), IoUtils.GetShaderPath("shader_ui.frag"));
        SkyboxShader = new Shader(IoUtils.GetShaderPath("shader_skybox.vert"), IoUtils.GetShaderPath("shader_skybox.frag"));
        CelestialBodyShader = new Shader(IoUtils.GetShaderPath("shader_celestial_body.vert"), IoUtils.GetShaderPath("shader_celestial_body.frag"));
    }
    
    
    public static void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        ProjectionMatrix = projectionMatrix;
        
        ShaderPlayer.Use();
        ShaderPlayer.SetMatrix4("projection", projectionMatrix);
        
        DebugShader.Use();
        DebugShader.SetMatrix4("projection", projectionMatrix);
        
        ShaderBlockOpaque.Use();
        ShaderBlockOpaque.SetMatrix4("projection", projectionMatrix);
        
        ShaderBlockCutout.Use();
        ShaderBlockCutout.SetMatrix4("projection", projectionMatrix);
        
        ShaderBlockTranslucent.Use();
        ShaderBlockTranslucent.SetMatrix4("projection", projectionMatrix);
        
        SkyboxShader.Use();
        SkyboxShader.SetMatrix4("projection", projectionMatrix);
        
        CelestialBodyShader.Use();
        CelestialBodyShader.SetMatrix4("projection", projectionMatrix);
    }
    
    
    public static void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        ViewMatrix = viewMatrix;
        
        ShaderPlayer.Use();
        ShaderPlayer.SetMatrix4("view", viewMatrix);
        
        DebugShader.Use();
        DebugShader.SetMatrix4("view", viewMatrix);
        
        ShaderBlockOpaque.Use();
        ShaderBlockOpaque.SetMatrix4("view", viewMatrix);
        
        ShaderBlockCutout.Use();
        ShaderBlockCutout.SetMatrix4("view", viewMatrix);
        
        ShaderBlockTranslucent.Use();
        ShaderBlockTranslucent.SetMatrix4("view", viewMatrix);
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
        ShaderBlockOpaque.Dispose();
        ShaderBlockCutout.Dispose();
        ShaderBlockTranslucent.Dispose();
        ShaderComposite.Dispose();
        ShaderScreen.Dispose();
        ShaderUi.Dispose();
        SkyboxShader.Dispose();
        CelestialBodyShader.Dispose();
        
        GC.SuppressFinalize(this);
    }
}