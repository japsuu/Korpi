namespace Korpi.Client.Blocks;

/// <summary>
/// The state of a <see cref="Block"/>. Also this is what chunks actually store.
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
    /// Cached info of which faces should not be meshed.
    /// 6 least-significant bits are flags for each face, in the order of: +x, +y, +z, -x, -y, -z.
    /// </summary>
    public byte InvisibleFaces { get; private set; }
    
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
        InvisibleFaces = 0b_00000000;
    }
    
    
    public void SetData(byte data)
    {
        Data = data;
    }
    
    
    /// <summary>
    /// Sets the visibility bit for the given face in the internal bitmask.
    /// </summary>
    /// <param name="faceNormal">Normal of the face</param>
    /// <param name="shouldBeVisible">If the face is visible</param>
    public void SetFaceVisibility(BlockFace faceNormal, bool shouldBeVisible)
    {
        // Update the correct bit in the mask. Remember that the 2 most significant bits are unused.
        if (shouldBeVisible)
            InvisibleFaces &= (byte)~(1 << (int)faceNormal);
        else
            InvisibleFaces |= (byte)(1 << (int)faceNormal);
    }
    
    
    public bool IsFaceVisible(BlockFace face)
    {
        // Return the correct bit in the mask. Remember that the 2 most significant bits are unused.
        return (InvisibleFaces & (1 << (int)face)) == 0;
    }
    
    
    public bool HasVisibleFaces()
    {
        return InvisibleFaces != 0b_00111111;
    }


    public static bool EqualsNonAlloc(BlockState b1, BlockState b2)
    {
        return b1.RenderType == b2.RenderType && b1.Id == b2.Id && b1.Data == b2.Data && b1.InvisibleFaces == b2.InvisibleFaces;
    }


    public override bool Equals(object? obj)
    {
        return obj is BlockState other && EqualsNonAlloc(this, other);
    }


    public override int GetHashCode()
    {
        return HashCode.Combine((int)RenderType, Id, Data, InvisibleFaces);
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
        return $"BlockState(Id={Id}, RenderType={RenderType}, Data={Data}, InvisibleFaces={InvisibleFaces})";
    }
}