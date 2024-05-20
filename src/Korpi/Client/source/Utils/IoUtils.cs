using Korpi.Client.Configuration;
using OpenTK.Windowing.Common.Input;
using StbImageSharp;

namespace Korpi.Client.Utils;

public static class IoUtils
{
    public static string GetTexturePath(string textureName) => Path.Combine(Constants.TEXTURE_PATH, textureName);
    
    
    public static string GetSkyboxTexturePath(string textureName) => GetTexturePath(Path.Combine("skybox", textureName));
    
    
    public static string GetSkyboxSunTexturePath(string textureName) => GetSkyboxTexturePath(Path.Combine("sun", textureName));
    
    
    public static string GetBlockTexturePath(string textureName) => GetTexturePath(Path.Combine("blocks", textureName));
    
    
    public static string GetBuiltinModFolderPath() => Path.Combine(Constants.MODS_PATH, "builtin");
    
    
    public static string GetBuiltinModPath() => Path.Combine(GetBuiltinModFolderPath(), $"builtin.{Constants.YAML_MOD_FILE_EXTENSION}");
    
    
    public static WindowIcon GetIcon()
    {
        string[] paths = {
            "assets/textures/icon/32.png",
        };
        Image[] images = new Image[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            StbImage.stbi_set_flip_vertically_on_load(0);

            using Stream stream = File.OpenRead(paths[i]);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            
            images[i] = new Image(image.Width, image.Height, image.Data);
        }
            
        return new WindowIcon(images);
    }
}