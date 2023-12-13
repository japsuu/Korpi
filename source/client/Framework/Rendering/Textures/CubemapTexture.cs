using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace BlockEngine.Framework.Rendering.Textures;

public class CubemapTexture : Texture
{
    public override TextureTarget TextureTarget => TextureTarget.TextureCubeMap;


    private CubemapTexture(int glHandle, string name) : base(glHandle, name)
    {
    }
    
    
    public static CubemapTexture LoadFromFile(string[] facesPaths, string texName)
    {
        if (facesPaths.Length != 6)
            throw new ArgumentException("Cubemap must have 6 textures.");

        // Generate handle
        int handle = GL.GenTexture();

        // Bind the handle
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, handle);


        for (int i = 0; i < facesPaths.Length; i++)
        {
            using Stream stream = File.OpenRead(facesPaths[i]);

            // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
            // so we tell StbImageSharp to flip the image when loading.
            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);

            GL.TexImage2D(
                TextureTarget.TextureCubeMapPositiveX + i,
                0,
                PixelInternalFormat.Rgb,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgb,
                PixelType.UnsignedByte,
                image.Data);
        }

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        return new CubemapTexture(handle, texName);
    }
}