using BlockEngine.Framework.Registries;

namespace BlockEngine.Framework.Blocks;

public static class BlockRegistry
{
    public static readonly Block Air = IdRegistry.Blocks.Register("be:air", new Block(BlockVisibility.Empty));
    public static readonly Block TestBlock = IdRegistry.Blocks.Register("be:test", new Block(BlockVisibility.Opaque));
}