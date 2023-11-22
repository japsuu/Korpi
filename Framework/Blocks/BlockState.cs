using BlockEngine.Framework.Meshing;
using BlockEngine.Utils;

namespace BlockEngine.Framework.Blocks;

/// <summary>
/// BlockState is what is stored in a dynamic block palette.
/// </summary>
public struct BlockState
{
    /// <summary>
    /// Reference to the block this state is for.
    /// </summary>
    public readonly Block Block;
    
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
    /// 6 most significant bits are flags for each neighbor, in the order of: +x, -x, +y, -y, +z, -z.
    /// 2 least significant bits are rotation data.
    /// </summary>
    public byte NeighbourMask { get; private set; }
    
    
    public bool IsValid => Block != null;


    public BlockState(Block block)
    {
        Block = block;
        Visibility = block.Visibility;
        Logger.Debug($"Create new state with visibility {Visibility}", 1000);
        Data = 0b00000000;
        NeighbourMask = 0b00000000;
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
    public void UpdateNeighborMask(BlockFaceNormal normal, bool hasNeighbor)
    {
        // Update the correct bit in the mask. Remember that the 2 least significant bits are rotation data.
        byte mask = (byte) (1 << (int) normal);
        if (hasNeighbor)
            NeighbourMask |= mask;
        else
            NeighbourMask &= (byte)~mask;
    }
    
    
    public void UpdateRotationBits(Orientation orientation)
    {
        NeighbourMask &= 0b11111100;
        NeighbourMask |= (byte) (orientation + 1);
    }
    
    
    public bool HasNeighbor(BlockFaceNormal orientation)
    {
        // Return the correct bit in the mask. Remember that the 2 least significant bits are rotation data.
        byte mask = (byte) (1 << (int) orientation);
        return (NeighbourMask & mask) != 0;
    }
    
    
    public Orientation GetRotation()
    {
        return (Orientation) (NeighbourMask & 0b00000011);
    }


    public bool Equals(BlockState other)
    {
        return Block.Equals(other.Block) && Visibility == other.Visibility && Data == other.Data && NeighbourMask == other.NeighbourMask;
    }


    public override bool Equals(object? obj)
    {
        return obj is BlockState other && Equals(other);
    }


    public override int GetHashCode()
    {
        return HashCode.Combine(Block, (int)Visibility, Data, NeighbourMask);
    }


    private sealed class BlockStateEqualityComparer : IEqualityComparer<BlockState>
    {
        public bool Equals(BlockState x, BlockState y)
        {
            return x.Block.Equals(y.Block) && x.Visibility == y.Visibility && x.Data == y.Data && x.NeighbourMask == y.NeighbourMask;
        }


        public int GetHashCode(BlockState obj)
        {
            return HashCode.Combine(obj.Block, (int)obj.Visibility, obj.Data, obj.NeighbourMask);
        }
    }

    public static IEqualityComparer<BlockState> BlockStateComparer { get; } = new BlockStateEqualityComparer();
}