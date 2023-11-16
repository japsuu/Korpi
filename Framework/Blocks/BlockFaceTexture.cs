using BlockEngine.Framework.Registries;

namespace BlockEngine.Framework.Blocks;

/// <summary>
/// Texture of one side of a <see cref="Block"/>.
/// </summary>
public class BlockFaceTexture : IHasId
{
    public ushort TextureAtlasIndex { get; private set; }

    
    public void AssignId(ushort id) => TextureAtlasIndex = id;
}