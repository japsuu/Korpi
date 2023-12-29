using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public static class ChunkHelper
{
    /// <summary>
    /// Contains the offsets of all 8 neighbouring columns of a column.
    /// </summary>
    public static readonly Vector2i[] ColumnNeighbourOffsets =
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
    /// Contains the offsets to all 26 neighbours of a cube.
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
        new(-1, -1, -1),// X-  Y-  Z-
    };


    static ChunkHelper()
    {
        ChunkNeighbourOffsets = new Vector3i[26];
        for (int i = 0; i < NeighbourOffsets.Length; i++)
        {
            ChunkNeighbourOffsets[i] = NeighbourOffsets[i] * Constants.CHUNK_SIZE;
        }
    }
}