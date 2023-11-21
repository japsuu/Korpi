﻿using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
using OpenTK.Mathematics;

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

        Vector3i pos = new(x, y, z);
        if (pos != new Vector3i(1, 1, 1) && pos != new Vector3i(BorderBlockIndex, 1, BorderBlockIndex) && pos != new Vector3i(BorderBlockIndex, 1, 1) && pos != new Vector3i(1, 1, BorderBlockIndex))
        {
            if (blockState.Visibility != BlockVisibility.Empty)
            {
                Logger.Debug("WALUIGI TIME"); 
            }
        }
    }
    
    
    /// <summary>
    /// If accessing multiple blocks, loop in the order of z, y, x.
    /// This minimizes cache trashing.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public BlockState GetData(int x, int y, int z)
    {
        int index = GetIndex(x, y, z);
        return Data[index];
    }


    private int GetIndex(int x, int y, int z)
    {
        // Calculate the index in a way that minimizes cache trashing.
        return x + Size * (y + Size * z);
    }
}