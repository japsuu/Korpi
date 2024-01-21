using Korpi.Client.Configuration;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Korpi.Client.Rendering.Textures;

/// <summary>
/// Represents a 2D texture array.<br/>
/// Images in this texture all are 2-dimensional. However, it contains multiple sets of 2-dimensional images,
/// all within one texture. The array length is part of the texture's size.
/// </summary>
public sealed class Texture2DArray : LayeredTexture
{
    public override string Name { get; }
    public override TextureTarget TextureTarget => TextureTarget.Texture2DArray;

    /// <summary>
    /// The width of the texture.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the texture.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// The number of layers.
    /// </summary>
    public int Layers { get; private set; }


    /// <summary>
    /// Allocates immutable texture storage with the given parameters.<br/>
    /// A value of zero for the number of mipmap levels will default to the maximum number of levels possible for the given bitmaps width and height.
    /// </summary>
    /// <param name="name">Name of the texture</param>
    /// <param name="internalFormat">The internal format to allocate.</param>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="layers">The number of layers to allocate.</param>
    /// <param name="levels">The number of mipmap levels.</param>
    public Texture2DArray(string name, SizedInternalFormat internalFormat, int width, int height, int layers, int levels = 0) :
        base(internalFormat, GetLevels(levels, width, height))
    {
        Name = name;
        Width = width;
        Height = height;
        Layers = layers;
        GL.BindTexture(TextureTarget, Handle);
        GL.TexStorage3D((TextureTarget3d)TextureTarget, Levels, InternalFormat, Width, Height, Layers);
        CheckError();
    }


    public static Texture2DArray LoadFromFiles(string[] paths, string texName)
    {
        Texture2DArray texture = new(texName, SizedInternalFormat.Rgba8, Constants.BLOCK_TEXTURE_SIZE, Constants.BLOCK_TEXTURE_SIZE, paths.Length);

        // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
        // so we tell StbImageSharp to flip the image when loading.
        StbImage.stbi_set_flip_vertically_on_load(1);

        for (int i = 0; i < paths.Length; i++)
        {
            using Stream stream = File.OpenRead(paths[i]);

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            if (image.Width != Constants.BLOCK_TEXTURE_SIZE)
                throw new ArgumentException($"Texture width must be {Constants.BLOCK_TEXTURE_SIZE} pixels.");

            if (image.Height != Constants.BLOCK_TEXTURE_SIZE)
                throw new ArgumentException($"Texture height must be {Constants.BLOCK_TEXTURE_SIZE} pixels.");

            GL.TexSubImage3D(
                TextureTarget.Texture2DArray,
                0,
                0,
                0,
                i,
                Constants.BLOCK_TEXTURE_SIZE,
                Constants.BLOCK_TEXTURE_SIZE,
                1,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                image.Data);
        }

        texture.SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Nearest);
        texture.SetWrapMode(TextureWrapMode.Repeat);
        texture.SetParameter(TextureParameterName.TextureMaxAnisotropy, Constants.ANISOTROPIC_FILTERING_LEVEL);
        texture.GenerateMipMaps();

        return texture;
    }
}