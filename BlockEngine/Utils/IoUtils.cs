namespace BlockEngine.Utils;

public static class IoUtils
{
    public static string GetShaderPath(string shaderName) => Path.Combine(Constants.SHADER_PATH, $"{shaderName}");
    
    
    public static string GetTexturePath(string textureName) => Path.Combine(Constants.TEXTURE_PATH, $"{textureName}");
    
    
    public static string GetSkyboxTexturePath(string textureName) => GetTexturePath(Path.Combine("skybox", $"{textureName}"));
    
    
    public static string GetBlockTexturePath(string textureName) => GetTexturePath(Path.Combine("blocks", $"{textureName}"));
}