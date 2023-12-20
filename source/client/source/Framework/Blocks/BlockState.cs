using BlockEngine.Client.Framework.Meshing;

namespace BlockEngine.Client.Framework.Blocks;

/// <summary>
/// BlockState is what chunks actually store.
/// </summary>
public struct BlockState
{
    /// <summary>
    /// Id of the block this state is for. The four MSB are rotation data.
    /// </summary>
    public ushort Id { get; private set; }
    
    /// <summary>
    /// How the block will be rendered.
    /// </summary>
    public readonly BlockRenderType RenderType;
    
    /// <summary>
    /// Data for the block. This is block-specific.
    /// </summary>
    public byte Data { get; private set; }
    
    /// <summary>
    /// Cached neighbor info.
    /// 6 most significant bits are flags for each neighbor, in the order of: +x, +y, +z, -x, -y, -z.
    /// 2 least significant bits are rotation data.
    /// </summary>
    public byte NeighbourMask { get; private set; }
    
    
    public bool IsAir => Id == 0;
    public bool IsRendered => RenderType != BlockRenderType.None;


    public BlockState(Block block)
    {
        Id = block.Id;
        RenderType = block.RenderType;
        Data = 0b_00000000;
        NeighbourMask = 0b_00000000;
    }
    
    
    public void SetData(byte data)
    {
        Data = data;
    }
    
    
    /// <summary>
    /// Sets the bit for the given neighbor in the internal bitmask.
    /// </summary>
    /// <param name="normal">Direction of the neighbour</param>
    /// <param name="hasNeighbor">If neighbour exists</param>
    public void SetNeighborMask(BlockFace normal, bool hasNeighbor)
    {
        // Update the correct bit in the mask. Remember that the 2 least significant bits are rotation data.
        byte mask = (byte) (1 << (int) normal);
        if (hasNeighbor)
            NeighbourMask |= mask;
        else
            NeighbourMask &= (byte)~mask;
    }
    
    
    public bool HasNeighbor(BlockFace orientation)
    {
        // Return the correct bit in the mask. Remember that the 2 least significant bits are rotation data.
        byte mask = (byte) (1 << (int) orientation);
        return (NeighbourMask & mask) != 0;
    }
    
    
    public void SetRotation(BlockOrientation blockOrientation)
    {
        // 2 least significant bits of NeighbourMask are the rotation data.
        NeighbourMask &= 0b11111100;
        NeighbourMask |= (byte) (blockOrientation + 1);
    }
    
    
    public BlockOrientation GetRotation()
    {
        // 2 least significant bits of NeighbourMask are the rotation data.
        return (BlockOrientation) ((NeighbourMask & 0b00000011) - 1);
    }


    public static bool EqualsNonAlloc(BlockState b1, BlockState b2)
    {
        return b1.RenderType == b2.RenderType && b1.Id == b2.Id && b1.Data == b2.Data && b1.NeighbourMask == b2.NeighbourMask;
    }


    public override bool Equals(object? obj)
    {
        return obj is BlockState other && EqualsNonAlloc(this, other);
    }


    public override int GetHashCode()
    {
        return HashCode.Combine((int)RenderType, Id, Data, NeighbourMask);
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
        return $"BlockState(Id={Id}, RenderType={RenderType}, Data={Data}, NeighbourMask={NeighbourMask})";
    }
}