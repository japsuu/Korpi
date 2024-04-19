namespace Korpi.Client.Blocks;

/// <summary>
/// The state of a <see cref="Block"/>. Also this is what chunks actually store.
/// </summary>
public struct BlockState
{
    /// <summary>
    /// Id of the block this state is for. The four MSB are rotation data.
    /// </summary>
    public readonly ushort Id;
    
    /// <summary>
    /// How the block will be rendered.
    /// </summary>
    public readonly BlockRenderType RenderType;
    
    /// <summary>
    /// Data for the block. This is block-specific.
    /// </summary>
    public byte Data { get; private set; }
    
    
    public bool IsAir => Id == 0;
    public bool IsRendered => RenderType != BlockRenderType.None;


    public BlockState(Block block)
    {
        Id = block.Id;
        RenderType = block.RenderType;
        Data = 0b_00000000;
    }
    
    
    public void SetData(byte data)
    {
        Data = data;
    }


    public static bool EqualsNonAlloc(BlockState b1, BlockState b2)
    {
        return b1.Id == b2.Id && b1.RenderType == b2.RenderType && b1.Data == b2.Data;
    }


    public override bool Equals(object? obj)
    {
        return obj is BlockState other && EqualsNonAlloc(this, other);
    }


    public override int GetHashCode()
    {
        return HashCode.Combine((int)RenderType, Id, Data);
    }
    
    
    public static bool operator ==(BlockState left, BlockState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlockState left, BlockState right)
    {
        return !(left == right);
    }


    public override string ToString()
    {
        return $"BlockState(Id={Id}, RenderType={RenderType}, Data={Data})";
    }
}