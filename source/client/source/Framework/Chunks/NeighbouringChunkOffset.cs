using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

/// <summary>
/// Represents the offset of a neighbouring chunk relative to the current chunk.
/// 
/// WARN: These must always be in the same order as the offsets in <see cref="ChunkHelper.ChunkNeighbourOffsets"/>
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

public static class NeighbouringChunkPositionExtensions
{
    public static Vector3i AsVec3I(this NeighbouringChunkOffset offset, Vector3i chunkPosition)
    {
        return ChunkHelper.ChunkNeighbourOffsets[(int)offset] + chunkPosition;
    }
}