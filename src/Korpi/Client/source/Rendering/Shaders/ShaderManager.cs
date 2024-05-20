using Korpi.Client.Configuration;
using Korpi.Client.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Logging;
using KorpiEngine.Rendering.Shaders.ShaderPrograms;

namespace Korpi.Client.Rendering.Shaders;

/// <summary>
/// Manages all shader programs.
/// 
/// Responsible for compiling and providing the shader programs.
/// 
/// TODO: Implement Uniform Buffer Objects (UBOs) for common uniforms: https://www.khronos.org/opengl/wiki/Uniform_Buffer_Objects
/// TODO: Abstract this class away? There's too many static references to the shader programs.
/// </summary>
public static class ShaderManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ShaderManager));
    
    public static PositionColorShaderProgram PositionColorShader { get; private set; } = null!;
    public static BlocksOpaqueCutoutShaderProgram BlockOpaqueCutoutShader { get; private set; } = null!;
    public static BlocksTranslucentShaderProgram BlockTranslucentShader { get; private set; } = null!;
    public static ScreenCompositeShaderProgram CompositeShader { get; private set; } = null!;
    public static UiPositionTexShaderProgram UiPositionTexShader { get; private set; } = null!;
    public static SkyboxShaderProgram SkyboxShader { get; private set; } = null!;
    public static CubemapShaderProgram CubemapTexShader { get; private set; } = null!;


    public static void Initialize()
    {
        CompileAllShaderPrograms();
    }


    private static void CompileAllShaderPrograms()
    {
        Logger.Info("Compiling all shader programs...");
        PositionColorShader = ShaderProgramFactory.Create<PositionColorShaderProgram>(Constants.SHADER_BASE_PATH);
        BlockOpaqueCutoutShader = ShaderProgramFactory.Create<BlocksOpaqueCutoutShaderProgram>(Constants.SHADER_BASE_PATH);
        BlockTranslucentShader = ShaderProgramFactory.Create<BlocksTranslucentShaderProgram>(Constants.SHADER_BASE_PATH);
        CompositeShader = ShaderProgramFactory.Create<ScreenCompositeShaderProgram>(Constants.SHADER_BASE_PATH);
        UiPositionTexShader = ShaderProgramFactory.Create<UiPositionTexShaderProgram>(Constants.SHADER_BASE_PATH);
        SkyboxShader = ShaderProgramFactory.Create<SkyboxShaderProgram>(Constants.SHADER_BASE_PATH);
        CubemapTexShader = ShaderProgramFactory.Create<CubemapShaderProgram>(Constants.SHADER_BASE_PATH);
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
    
    
    public static void Dispose()
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