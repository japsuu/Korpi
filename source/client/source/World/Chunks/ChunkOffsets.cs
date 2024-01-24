using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

public static class ChunkOffsets
{
    /// <summary>
    /// Represents the offset to a neighbour on a regular grid, as flags.
    /// </summary>
    [Flags]
    public enum NeighbourOffsetFlags
    {
        None = 0,
        
        // 6 Faces
        FaceXPos = 1 << 0,           // X+
        FaceYPos = 1 << 1,           //     Y+
        FaceZPos = 1 << 2,           //         Z+
        FaceXNeg = 1 << 3,           // X-
        FaceYNeg = 1 << 4,           //     Y-
        FaceZNeg = 1 << 5,           //         Z-
    
        // 12 Edges
        // Top
        EdgeXPosYPos = 1 << 6,       // X+  Y+
        EdgeXNegYPos = 1 << 7,       // X-  Y+
        EdgeYPosZPos = 1 << 8,       //     Y+  Z+
        EdgeYPosZNeg = 1 << 9,       //     Y+  Z-
        // Middle
        EdgeXPosZPos = 1 << 10,       // X+      Z+
        EdgeXPosZNeg = 1 << 11,       // X+      Z-
        EdgeXNegZPos = 1 << 12,       // X-      Z+
        EdgeXNegZNeg = 1 << 13,       // X-      Z-
        // Bottom
        EdgeXPosYNeg = 1 << 14,       // X+  Y-
        EdgeXNegYNeg = 1 << 15,       // X-  Y-
        EdgeYNegZPos = 1 << 16,       //     Y-  Z+
        EdgeYNegZNeg = 1 << 17,       //     Y-  Z-
    
        // 8 Corners
        CornerXPosYPosZPos = 1 << 18, // X+  Y+  Z+
        CornerXPosYPosZNeg = 1 << 19, // X+  Y+  Z-
        CornerXNegYPosZPos = 1 << 20, // X-  Y+  Z+
        CornerXNegYPosZNeg = 1 << 21, // X-  Y+  Z-
        CornerXPosYNegZPos = 1 << 22, // X+  Y-  Z+
        CornerXPosYNegZNeg = 1 << 23, // X+  Y-  Z-
        CornerXNegYNegZPos = 1 << 24, // X-  Y-  Z+
        CornerXNegYNegZNeg = 1 << 25, // X-  Y-  Z-
    }

    
    /// <summary>
    /// Contains the offsets to all 26 neighbours of a cube.
    /// 
    /// Warn: These must always be in the same order as the offsets in <see cref="NeighbourOffsetFlags"/>
    /// </summary>
    public static readonly Vector3i[] NeighbourOffsets =
    {
        // 6 Faces
        new(1, 0, 0),   // X+
        new(0, 1, 0),   //     Y+
        new(0, 0, 1),   //         Z+
        new(-1, 0, 0),  // X-
        new(0, -1, 0),  //     Y-
        new(0, 0, -1),  //         Z-

        // 12 Edges
        new(1, 1, 0),   // X+  Y+
        new(-1, 1, 0),  // X-  Y+
        new(0, 1, 1),   //     Y+  Z+
        new(0, 1, -1),  //     Y+  Z-
        new(1, 0, 1),   // X+      Z+
        new(1, 0, -1),  // X+      Z-
        new(-1, 0, 1),  // X-      Z+
        new(-1, 0, -1), // X-      Z-
        new(1, -1, 0),  // X+  Y-
        new(-1, -1, 0), // X-  Y-
        new(0, -1, 1),  //     Y-  Z+
        new(0, -1, -1), //     Y-  Z-

        // 8 Corners
        new(1, 1, 1),   // X+  Y+  Z+
        new(1, 1, -1),  // X+  Y+  Z-
        new(-1, 1, 1),  // X-  Y+  Z+
        new(-1, 1, -1), // X-  Y+  Z-
        new(1, -1, 1),  // X+  Y-  Z+
        new(1, -1, -1), // X+  Y-  Z-
        new(-1, -1, 1), // X-  Y-  Z+
        new(-1, -1, -1) // X-  Y-  Z-
    };
    
    /// <summary>
    /// Contains the offsets of all 8 neighbouring columns of a column.
    /// </summary>
    public static readonly Vector2i[] RegionNeighbourOffsets =
    {
        // 4 Corners
        new(1 * Constants.SUBCHUNK_SIDE_LENGTH, 1 * Constants.SUBCHUNK_SIDE_LENGTH),
        new(-1 * Constants.SUBCHUNK_SIDE_LENGTH, 1 * Constants.SUBCHUNK_SIDE_LENGTH),
        new(-1 * Constants.SUBCHUNK_SIDE_LENGTH, -1 * Constants.SUBCHUNK_SIDE_LENGTH),
        new(1 * Constants.SUBCHUNK_SIDE_LENGTH, -1 * Constants.SUBCHUNK_SIDE_LENGTH),

        // 4 Faces
        new(1 * Constants.SUBCHUNK_SIDE_LENGTH, 0 * Constants.SUBCHUNK_SIDE_LENGTH),
        new(0 * Constants.SUBCHUNK_SIDE_LENGTH, 1 * Constants.SUBCHUNK_SIDE_LENGTH),
        new(-1 * Constants.SUBCHUNK_SIDE_LENGTH, 0 * Constants.SUBCHUNK_SIDE_LENGTH),
        new(0 * Constants.SUBCHUNK_SIDE_LENGTH, -1 * Constants.SUBCHUNK_SIDE_LENGTH),
    };

    /// <summary>
    /// Contains the offsets of all 26 neighbouring chunks of a chunk.
    /// </summary>
    public static readonly Vector3i[] ChunkNeighbourOffsets;
    
    
    /// <summary>
    /// Yields the vector of every neighbour that is flagged in <paramref name="flags"/>.
    /// </summary>
    public static IEnumerable<Vector3i> OffsetsAsVectors(NeighbourOffsetFlags flags)
    {
        if (flags == NeighbourOffsetFlags.None)
            yield break;
        
        for (int i = 0; i < 26; i++)
        {
            Vector3i offset = NeighbourOffsets[i];
            if (flags.HasFlag((NeighbourOffsetFlags)(1 << i)))
            {
                yield return offset;
            }
        }
    }
    
    
    /// <summary>
    /// Yields the vector of every neighbour that is flagged in <paramref name="flags"/>.
    /// </summary>
    public static IEnumerable<Vector3i> OffsetsAsChunkVectors(NeighbourOffsetFlags flags)
    {
        if (flags == NeighbourOffsetFlags.None)
            yield break;
        
        for (int i = 0; i < 26; i++)
        {
            Vector3i offset = ChunkNeighbourOffsets[i];
            if (flags.HasFlag((NeighbourOffsetFlags)(1 << i)))
            {
                yield return offset;
            }
        }
    }


    /// <summary>
    /// Calculates which chunk neighbours of the given block position would be affected by a block change.
    /// In worst case, a block change in the corner of a chunk can affect 7 neighbours.
    /// </summary>
    /// <param name="position">Position of the block that was changed</param>
    /// <returns>Flags indicating which neighbouring chunks are affected</returns>
    public static NeighbourOffsetFlags CalculateNeighboursFromOtherChunks(SubChunkBlockPosition position)
    {
        bool isXPositive = position.X == Constants.SUBCHUNK_SIDE_LENGTH - 1;
        bool isXNegative = position.X == 0;
        bool isYPositive = position.Y == Constants.SUBCHUNK_SIDE_LENGTH - 1;
        bool isYNegative = position.Y == 0;
        bool isZPositive = position.Z == Constants.SUBCHUNK_SIDE_LENGTH - 1;
        bool isZNegative = position.Z == 0;
        
        NeighbourOffsetFlags affectedNeighbours = NeighbourOffsetFlags.None;
        
        // Check 6 faces. Three faces can be affected at a time.
        // X faces
        if (isXPositive)
            affectedNeighbours |= NeighbourOffsetFlags.FaceXPos;
        else if (isXNegative)
            affectedNeighbours |= NeighbourOffsetFlags.FaceXNeg;
        
        // Y faces
        if (isYPositive)
            affectedNeighbours |= NeighbourOffsetFlags.FaceYPos;
        else if (isYNegative)
            affectedNeighbours |= NeighbourOffsetFlags.FaceYNeg;
        
        // Z faces
        if (isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.FaceZPos;
        else if (isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.FaceZNeg;
        
        // Check 12 edges. Three edges can be affected at a time.
        // Top
        if (isXPositive && isYPositive)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXPosYPos;
        else if (isXNegative && isYPositive)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXNegYPos;
        else if (isYPositive && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeYPosZPos;
        else if (isYPositive && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeYPosZNeg;
        
        // Middle
        if (isXPositive && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXPosZPos;
        else if (isXPositive && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXPosZNeg;
        else if (isXNegative && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXNegZPos;
        else if (isXNegative && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXNegZNeg;
        
        // Bottom
        if (isXPositive && isYNegative)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXPosYNeg;
        else if (isXNegative && isYNegative)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeXNegYNeg;
        else if (isYNegative && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeYNegZPos;
        else if (isYNegative && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.EdgeYNegZNeg;
        
        // Check 8 corners. Only one corner can be affected at a time.
        if (isXPositive && isYPositive && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXPosYPosZPos;
        else if (isXPositive && isYPositive && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXPosYPosZNeg;
        else if (isXNegative && isYPositive && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXNegYPosZPos;
        else if (isXNegative && isYPositive && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXNegYPosZNeg;
        else if (isXPositive && isYNegative && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXPosYNegZPos;
        else if (isXPositive && isYNegative && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXPosYNegZNeg;
        else if (isXNegative && isYNegative && isZPositive)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXNegYNegZPos;
        else if (isXNegative && isYNegative && isZNegative)
            affectedNeighbours |= NeighbourOffsetFlags.CornerXNegYNegZNeg;
        
        return affectedNeighbours;
    }


    static ChunkOffsets()
    {
        ChunkNeighbourOffsets = new Vector3i[26];
        for (int i = 0; i < 26; i++)
        {
            ChunkNeighbourOffsets[i] = NeighbourOffsets[i] * Constants.SUBCHUNK_SIDE_LENGTH;
        }
    }
}