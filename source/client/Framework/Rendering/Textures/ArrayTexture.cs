using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace BlockEngine.Client.Framework.Rendering.Textures;

public class ArrayTexture : Texture
{
    public override TextureTarget TextureTarget => TextureTarget.Texture2DArray;
    
    
    private ArrayTexture(int glHandle, string name) : base(glHandle, name)
    {
    }
    
    
    public static ArrayTexture LoadFromFiles(string[] paths, string texName, int mipLevelCount = 2)
    {
        // Generate handle
        int handle = GL.GenTexture();

        // Bind the handle
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, handle);
        
        // Allocate the storage.
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, mipLevelCount, SizedInternalFormat.Rgba8, Constants.BLOCK_TEXTURE_SIZE, Constants.BLOCK_TEXTURE_SIZE, paths.Length);

        // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
        // so we tell StbImageSharp to flip the image when loading.
        StbImage.stbi_set_flip_vertically_on_load(1);


        for (int i = 0; i < paths.Length; i++)
        {
            using Stream stream = File.OpenRead(paths[i]);
            
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, Constants.BLOCK_TEXTURE_SIZE, Constants.BLOCK_TEXTURE_SIZE, 1, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }

        // First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxAnisotropy, Constants.ANISOTROPIC_FILTERING_LEVEL);

        // Next, generate mipmaps.
        if (mipLevelCount > 1)
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        }

        return new ArrayTexture(handle, texName);
    }
}