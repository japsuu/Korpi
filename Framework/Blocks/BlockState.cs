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
    public byte MeshCache { get; private set; }


    public BlockState(Block block)
    {
        Block = block;
        Visibility = block.Visibility;
        Data = 0b00000000;
        MeshCache = 0b00000000;
    }
    
    
    public void SetData(byte data)
    {
        Data = data;
    }
    
    
    public void UpdateNeighborBits(Orientation orientation, bool isNeighbor)
    {
        byte mask = (byte) (1 << (int) orientation);
        if (isNeighbor)
        {
            MeshCache |= mask;
        }
        else
        {
            MeshCache &= (byte) ~mask;
        }
    }
    
    
    public void UpdateRotationBits(Orientation orientation)
    {
        MeshCache &= 0b11111100;
        MeshCache |= (byte) (orientation + 1);
    }
    
    
    public bool HasNeighbor(Orientation orientation)
    {
        byte mask = (byte) (1 << (int) orientation);
        return (MeshCache & mask) != 0;
    }
    
    
    public Orientation GetRotation()
    {
        return (Orientation) (MeshCache & 0b00000011);
    }
}