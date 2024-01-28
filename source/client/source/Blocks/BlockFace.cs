using OpenTK.Mathematics;

namespace Korpi.Client.Blocks;

/// <summary>
/// Represents a face of a block.
/// Multiple classes are dependant on the order of the values.
/// </summary>
public enum BlockFace
{
    /// <summary>
    /// North.
    /// </summary>
    XPositive = 0,
    
    /// <summary>
    /// Up.
    /// </summary>
    YPositive = 1,
    
    /// <summary>
    /// East.
    /// </summary>
    ZPositive = 2,
    
    /// <summary>
    /// South.
    /// </summary>
    XNegative = 3,
    
    /// <summary>
    /// Down.
    /// </summary>
    YNegative = 4,
    
    /// <summary>
    /// West.
    /// </summary>
    ZNegative = 5
}


public static class BlockFaceExtensions
{
    public static Vector3i Normal(this BlockFace face)
    {
        return face switch
        {
            BlockFace.XPositive => new Vector3i(1, 0, 0),
            BlockFace.YPositive => new Vector3i(0, 1, 0),
            BlockFace.ZPositive => new Vector3i(0, 0, 1),
            BlockFace.XNegative => new Vector3i(-1, 0, 0),
            BlockFace.YNegative => new Vector3i(0, -1, 0),
            BlockFace.ZNegative => new Vector3i(0, 0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }
}