namespace Korpi.Client.Blocks.Textures;

/// <summary>
/// Texture of one side of a <see cref="Block"/>.
/// </summary>
public class SingleBlockFaceTexture : IBlockFaceTexture
{
    protected ushort TextureIndex;


    public SingleBlockFaceTexture(ushort textureIndex)
    {
        TextureIndex = textureIndex;
    }
    
    
    public ushort GetId() => TextureIndex;
}