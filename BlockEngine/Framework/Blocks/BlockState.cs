using BlockEngine.Framework.Meshing;
using BlockEngine.Utils;

namespace BlockEngine.Framework.Blocks;

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
    public readonly BlockVisibility Visibility;
    
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
    
    
    public BlockState(Block block)
    {
        Id = block.Id;
        Visibility = block.Visibility;
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
    public void SetNeighborMask(BlockFaceNormal normal, bool hasNeighbor)
    {
        // Update the correct bit in the mask. Remember that the 2 least significant bits are rotation data.
        byte mask = (byte) (1 << (int) normal);
        if (hasNeighbor)
            NeighbourMask |= mask;
        else
            NeighbourMask &= (byte)~mask;
    }
    
    
    public bool HasNeighbor(BlockFaceNormal orientation)
    {
        // Return the correct bit in the mask. Remember that the 2 least significant bits are rotation data.
        byte mask = (byte) (1 << (int) orientation);
        return (NeighbourMask & mask) != 0;
    }
    
    
    public void SetRotation(Orientation orientation)
    {
        // 2 least significant bits of NeighbourMask are the rotation data.
        NeighbourMask &= 0b11111100;
        NeighbourMask |= (byte) (orientation + 1);
    }
    
    
    public Orientation GetRotation()
    {
        // 2 least significant bits of NeighbourMask are the rotation data.
        return (Orientation) ((NeighbourMask & 0b00000011) - 1);
    }


    public bool Equals(BlockState other)
    {
        return Visibility == other.Visibility && Id == other.Id && Data == other.Data && NeighbourMask == other.NeighbourMask;
    }


    public override bool Equals(object? obj)
    {
        return obj is BlockState other && Equals(other);
    }


    public override int GetHashCode()
    {
        return HashCode.Combine((int)Visibility, Id, Data, NeighbourMask);
    }
    
    
    public static bool operator ==(BlockState left, BlockState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlockState left, BlockState right)
    {
        return !(left == right);
    }
}