namespace BlockEngine.Client.Framework.Blocks.Textures;

/// <summary>
/// Texture of one side of a <see cref="Block"/>.
/// </summary>
public class BlockFaceTexture
{
    protected ushort TextureIndex;


    public BlockFaceTexture(ushort textureIndex)
    {
        TextureIndex = textureIndex;
    }
    
    
    public virtual ushort GetTextureIndex() => TextureIndex;
}