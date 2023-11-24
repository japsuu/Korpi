using BlockEngine.Framework.Blocks;

namespace BlockEngine.Framework.Registries;

public static class IdRegistry
{
    public static readonly NamespacedIdRegistry<Block> Blocks = new("Blocks");
    public static readonly IndexedRegistry<BlockFaceTexture> BlockFaceTextures = new("BlockFaceTextures");
}

public static class BlockTextureDatabase
{
    
}