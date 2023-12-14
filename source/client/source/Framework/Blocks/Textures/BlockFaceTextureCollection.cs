using BlockEngine.Client.Framework.Meshing;

namespace BlockEngine.Client.Framework.Blocks.Textures;

/// <summary>
/// All face textures of a <see cref="Block"/>.
/// </summary>
public class BlockFaceTextureCollection
{
    private readonly BlockFaceTexture[] _textures;
    
    
    public BlockFaceTextureCollection(BlockFaceTexture[] textures)
    {
        _textures = textures;
    }
    
    
    public ushort GetTextureIndex(BlockFace face) => _textures[(int)face].GetTextureIndex();
}