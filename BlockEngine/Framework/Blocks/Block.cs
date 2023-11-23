using BlockEngine.Framework.Registries;

namespace BlockEngine.Framework.Blocks;

/// <summary>
/// Blocks are what define the unique functionality of a block. It has functions to override the default behavior of a block.
/// Blocks are stored only once in memory, and are accessed through a reference if needed.
/// BlockState is what is stored in a dynamic palette.
/// </summary>
public class Block : IHasId
{
    public ushort Id { get; private set; }
    public readonly BlockVisibility Visibility;
    
    private readonly BlockState _defaultState;


    public Block(ushort id, BlockVisibility visibility)
    {
        Id = id;
        Visibility = visibility;
        _defaultState = new BlockState(this);
    }


    public void AssignId(ushort id)
    {
        Id = id;
    }
    
    
    public BlockState GetDefaultState()
    {
        return _defaultState;
    }
}

public enum Orientation
{
    /// <summary>
    /// X+ axis.
    /// </summary>
    North = 0,
    
    /// <summary>
    /// X- axis.
    /// </summary>
    South = 1,
    
    /// <summary>
    /// Z+ axis.
    /// </summary>
    East = 2,
    
    /// <summary>
    /// Z- axis.
    /// </summary>
    West = 3,
}