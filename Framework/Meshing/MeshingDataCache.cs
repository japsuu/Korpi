using BlockEngine.Framework.Blocks;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Contains the BlockState data of a chunk, and a border of its neighbouring chunks.
/// </summary>
public class MeshingDataCache
{
    public readonly BlockState[] Data;
    public readonly int Size;
    public readonly int BorderBlockIndex;


    public MeshingDataCache(int size)
    {
        Size = size + 2;
        BorderBlockIndex = Size - 1;
        
        Data = new BlockState[Size * Size * Size];
    }


    public void SetData(int x, int y, int z, BlockState blockState)
    {
        Data[GetIndex(x, y, z)] = blockState;
    }
    
    
    public BlockState GetData(int x, int y, int z)
    {
        return Data[GetIndex(x, y, z)];
    }
    
    
    private int GetIndex(int x, int y, int z)
    {
        return x + Size * (y + Size * z);
    }
}