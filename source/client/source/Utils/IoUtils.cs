namespace Korpi.Client.Utils;

public static class IoUtils
{
    public static string GetShaderPath(string shaderName) => Path.Combine(Constants.SHADER_PATH, shaderName);
    
    
    public static string GetTexturePath(string textureName) => Path.Combine(Constants.TEXTURE_PATH, textureName);
    
    
    public static string GetSkyboxTexturePath(string textureName) => GetTexturePath(Path.Combine("skybox", textureName));
    
    
    public static string GetSkyboxSunTexturePath(string textureName) => GetSkyboxTexturePath(Path.Combine("sun", textureName));
    
    
    public static string GetBlockTexturePath(string textureName) => GetTexturePath(Path.Combine("blocks", textureName));
    
    
    public static string GetBuiltinModFolderPath() => Path.Combine(Constants.MODS_PATH, "builtin");
    
    
    public static string GetBuiltinModPath() => Path.Combine(GetBuiltinModFolderPath(), $"builtin.{Constants.YAML_MOD_FILE_EXTENSION}");
}