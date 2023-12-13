﻿namespace BlockEngine.Framework.Blocks;

[Obsolete("Please use BlockFace instead")]
public enum BlockOrientation
{
    /// <summary>
    /// X+ axis.
    /// </summary>
    North = 0,
    
    /// <summary>
    /// X- axis.
    /// </summary>
    South = 1,
    
    /// <summary>
    /// Z+ axis.
    /// </summary>
    East = 2,
    
    /// <summary>
    /// Z- axis.
    /// </summary>
    West = 3,
}