using Korpi.Client.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Logging;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Shaders;

/// <summary>
/// Manages all shader programs.
/// 
/// The manager is responsible for compiling and providing the shader programs.
/// 
/// The manager also keeps track of the projection and view matrices and provides the
/// screen position of a world position.
/// 
/// The manager also provides events for when the projection and view matrices are changed.
/// TODO: Implement Uniform Buffer Objects (UBOs) for common uniforms: https://www.khronos.org/opengl/wiki/Uniform_Buffer_Objects
/// TODO: Abstract this class away? There's too many static references to the shader programs.
/// </summary>
public static class ShaderManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ShaderManager));
    
    public static event Action<Matrix4>? ProjectionMatrixChanged;
    public static event Action<Matrix4>? ViewMatrixChanged;
    
    public static PositionColorShaderProgram PositionColorShader { get; private set; } = null!;
    public static BlocksOpaqueCutoutShaderProgram BlockOpaqueCutoutShader { get; private set; } = null!;
    public static BlocksTranslucentShaderProgram BlockTranslucentShader { get; private set; } = null!;
    public static ScreenCompositeShaderProgram CompositeShader { get; private set; } = null!;
    public static UiPositionTexShaderProgram UiPositionTexShader { get; private set; } = null!;
    public static SkyboxShaderProgram SkyboxShader { get; private set; } = null!;
    public static CubemapShaderProgram CubemapTexShader { get; private set; } = null!;

    private static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;
    private static Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;


    public static void Initialize()
    {
        CompileAllShaderPrograms();
        ClientWindow.Disposing += Dispose;
    }


    private static void CompileAllShaderPrograms()
    {
        Logger.Info("Compiling all shader programs...");
        PositionColorShader = ShaderProgramFactory.Create<PositionColorShaderProgram>();
        BlockOpaqueCutoutShader = ShaderProgramFactory.Create<BlocksOpaqueCutoutShaderProgram>();
        BlockTranslucentShader = ShaderProgramFactory.Create<BlocksTranslucentShaderProgram>();
        CompositeShader = ShaderProgramFactory.Create<ScreenCompositeShaderProgram>();
        UiPositionTexShader = ShaderProgramFactory.Create<UiPositionTexShaderProgram>();
        SkyboxShader = ShaderProgramFactory.Create<SkyboxShaderProgram>();
        CubemapTexShader = ShaderProgramFactory.Create<CubemapShaderProgram>();
        Logger.Info("All shader programs compiled.");
    }


    public static void ReloadAllShaderPrograms()
    {
        Logger.Info("Reloading all shader programs...");
        PositionColorShader.Dispose();
        BlockOpaqueCutoutShader.Dispose();
        BlockTranslucentShader.Dispose();
        CompositeShader.Dispose();
        UiPositionTexShader.Dispose();
        SkyboxShader.Dispose();
        CubemapTexShader.Dispose();
        
        CompileAllShaderPrograms();
    }
    
    
    public static void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        ProjectionMatrix = projectionMatrix;
        ProjectionMatrixChanged?.Invoke(projectionMatrix);
    }
    
    
    public static void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        ViewMatrix = viewMatrix;
        ViewMatrixChanged?.Invoke(viewMatrix);
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
        screenCoordinates.X *= WindowInfo.ClientWidth;
        screenCoordinates.Y *= WindowInfo.ClientHeight;
        screenPos = screenCoordinates;
        return true;
    }
    
    
    private static void Dispose()
    {
        PositionColorShader.Dispose();
        BlockOpaqueCutoutShader.Dispose();
        BlockTranslucentShader.Dispose();
        CompositeShader.Dispose();
        UiPositionTexShader.Dispose();
        SkyboxShader.Dispose();
        CubemapTexShader.Dispose();
    }
}