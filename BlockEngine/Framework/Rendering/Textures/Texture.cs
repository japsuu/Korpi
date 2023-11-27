using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace BlockEngine.Framework.Rendering.Textures;

public class Texture
{
    public readonly int Handle;


    public Texture(int glHandle)
    {
        Handle = glHandle;
    }


    public static Texture LoadFromFile(string path)
    {
        // Generate handle
        int handle = GL.GenTexture();

        // Bind the handle
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, handle);
        
        // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
        // so we tell StbImageSharp to flip the image when loading.
        StbImage.stbi_set_flip_vertically_on_load(1);

        // Here we open a stream to the file and pass it to StbImageSharp to load.
        using (Stream stream = File.OpenRead(path))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
        // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy, Constants.ANISOTROPIC_FILTERING_LEVEL);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        return new Texture(handle);
    }


    // Activate texture
    // Multiple textures can be bound, if your shader needs more than just one.
    // If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
    // The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.
    public void Use(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
    
    
    public void Dispose()
    {
        GL.DeleteTexture(Handle);
    }
}