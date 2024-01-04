using OpenTK.Mathematics;

namespace BlockEngine.Client.World.Regions.Chunks;

public static class ChunkOffsets
{
    /// <summary>
    /// Represents the offset of a neighbouring chunk relative to the current chunk.
    /// 
    /// WARN: These must always be in the same order as the offsets in <see cref="ChunkOffsets.NeighbourOffsets"/>
    /// </summary>
    public enum NeighbouringChunkOffset
    {
        // 6 Faces
        FaceXPos,           // X+
        FaceYPos,           //     Y+
        FaceZPos,           //         Z+
        FaceXNeg,           // X-
        FaceYNeg,           //     Y-
        FaceZNeg,           //         Z-
    
        // 12 Edges
        // Top
        EdgeXPosYPos,       // X+  Y+
        EdgeXNegYPos,       // X-  Y+
        EdgeYPosZPos,       //     Y+  Z+
        EdgeYPosZNeg,       //     Y+  Z-
        // Middle
        EdgeXPosZPos,       // X+      Z+
        EdgeXPosZNeg,       // X+      Z-
        EdgeXNegZPos,       // X-      Z+
        EdgeXNegZNeg,       // X-      Z-
        // Bottom
        EdgeXPosYNeg,       // X+  Y-
        EdgeXNegYNeg,       // X-  Y-
        EdgeYNegZPos,       //     Y-  Z+
        EdgeYNegZNeg,       //     Y-  Z-
    
        // 8 Corners
        CornerXPosYPosZPos, // X+  Y+  Z+
        CornerXPosYPosZNeg, // X+  Y+  Z-
        CornerXNegYPosZPos, // X-  Y+  Z+
        CornerXNegYPosZNeg, // X-  Y+  Z-
        CornerXPosYNegZPos, // X+  Y-  Z+
        CornerXPosYNegZNeg, // X+  Y-  Z-
        CornerXNegYNegZPos, // X-  Y-  Z+
        CornerXNegYNegZNeg, // X-  Y-  Z-
    }

    
    /// <summary>
    /// Contains the offsets to all 26 neighbours of a cube.
    /// 
    /// WARN: These must always be in the same order as the offsets in <see cref="ChunkOffsets.NeighbouringChunkOffset"/>
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
        new(1 * Constants.CHUNK_SIZE, 1 * Constants.CHUNK_SIZE),
        new(-1 * Constants.CHUNK_SIZE, 1 * Constants.CHUNK_SIZE),
        new(-1 * Constants.CHUNK_SIZE, -1 * Constants.CHUNK_SIZE),
        new(1 * Constants.CHUNK_SIZE, -1 * Constants.CHUNK_SIZE),

        // 4 Faces
        new(1 * Constants.CHUNK_SIZE, 0 * Constants.CHUNK_SIZE),
        new(0 * Constants.CHUNK_SIZE, 1 * Constants.CHUNK_SIZE),
        new(-1 * Constants.CHUNK_SIZE, 0 * Constants.CHUNK_SIZE),
        new(0 * Constants.CHUNK_SIZE, -1 * Constants.CHUNK_SIZE),
    };

    /// <summary>
    /// Contains the offsets of all 26 neighbouring chunks of a chunk.
    /// </summary>
    public static readonly Vector3i[] ChunkNeighbourOffsets;
    
    
    /// <summary>
    /// Gets the vector representing the offset of the given neighbour relative to the current chunk.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="chunkPosition"></param>
    /// <returns></returns>
    public static Vector3i OffsetAsVector(NeighbouringChunkOffset offset, Vector3i chunkPosition) => ChunkNeighbourOffsets[(int)offset] + chunkPosition;


    static ChunkOffsets()
    {
        ChunkNeighbourOffsets = new Vector3i[26];
        for (int i = 0; i < NeighbourOffsets.Length; i++)
        {
            ChunkNeighbourOffsets[i] = NeighbourOffsets[i] * Constants.CHUNK_SIZE;
        }
    }
}