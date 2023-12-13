namespace BlockEngine.Client.Framework.Obsolete;

/// <summary>
/// Indexes ushorts to a flattened 3D array.
/// </summary>
[Obsolete("Use BlockPalette instead.")]
public class IdStorage
{
    public readonly ushort[] Ids = new ushort[Constants.CHUNK_SIZE_CUBED];


    public ushort this[int index]
    {
        get => Ids[index];
        set => Ids[index] = value;
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public ushort this[int x, int y, int z]
    {
        // Use (z << (W + H)) + (y << W) + x to index into a 3D array stored as a 1D array.
        // So, in this formula, "W" and "H" are the logarithms (base 2) of the dimensions of the array in the x and y directions, respectively.
        // In other words, if the width and height of the 3D array are both 2^N, then "W" and "H" would be equal to "N."
        get => Ids[(z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (y << Constants.CHUNK_SIZE_LOG2) + x];
        set => Ids[(z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (y << Constants.CHUNK_SIZE_LOG2) + x] = value;
    }
}