using BlockEngine.Framework.Blocks.Serialization;
using BlockEngine.Framework.Blocks.Textures;
using BlockEngine.Framework.Registries;

namespace BlockEngine.Framework.Blocks;

/// <summary>
/// Blocks are what define the unique functionality of a block. It has functions to override the default behavior of a block.
/// Blocks are stored only once in memory, and are accessed through a reference if needed.
/// BlockState is what is stored in a dynamic palette.
/// </summary>
public class Block
{
    public readonly ushort Id;
    public readonly string? Name;
    public readonly BlockRenderType RenderType;
    
    private readonly BlockFaceTextureCollection? _textures;
    private readonly BlockState _defaultState;


    public Block(ushort id, string name, BlockRenderType renderType, BlockFaceTextureCollection? textures)
    {
        Id = id;
        Name = name;
        RenderType = renderType;
        _textures = textures;
        _defaultState = new BlockState(this);
    }
    
    
    public BlockState GetDefaultState()
    {
        return _defaultState;
    }
}