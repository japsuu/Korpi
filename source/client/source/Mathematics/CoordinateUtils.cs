using System.Runtime.CompilerServices;
using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.Mathematics;

public static class CoordinateUtils
{
    private const int SUBCHUNK_SIDE_LENGTH_BITMASK = Constants.SUBCHUNK_SIDE_LENGTH - 1;
    
    
    /// <summary>
    /// World position -> chunk position.
    /// Example: (36, 74, -5) -> (32, 64, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk grid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i WorldToSubChunk(Vector3 position)
    {
        return new Vector3i(
            (int)Math.Floor(position.X) & ~SUBCHUNK_SIDE_LENGTH_BITMASK,
            (int)Math.Floor(position.Y) & ~SUBCHUNK_SIDE_LENGTH_BITMASK,
            (int)Math.Floor(position.Z) & ~SUBCHUNK_SIDE_LENGTH_BITMASK
        );
    }


    /// <summary>
    /// World position -> chunk position.
    /// Example: (36, 74, -5) -> (32, 64, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk grid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i WorldToSubChunk(Vector3i position)
    {
        return new Vector3i(
            position.X & ~SUBCHUNK_SIDE_LENGTH_BITMASK,
            position.Y & ~SUBCHUNK_SIDE_LENGTH_BITMASK,
            position.Z & ~SUBCHUNK_SIDE_LENGTH_BITMASK
        );
    }
        
    
    /// <summary>
    /// World position -> chunk column position.
    /// Example: (36, 74, -5) -> (32, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk column grid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i WorldToColumn(Vector3i position)
    {
        return new Vector2i(
            position.X & ~SUBCHUNK_SIDE_LENGTH_BITMASK,
            position.Z & ~SUBCHUNK_SIDE_LENGTH_BITMASK
        );
    }
        
    
    /// <summary>
    /// World position -> chunk column position.
    /// Example: (36, 74, -5) -> (32, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk column grid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i WorldToColumn(Vector3 position)
    {
        return new Vector2i(
            (int)Math.Floor(position.X) & ~SUBCHUNK_SIDE_LENGTH_BITMASK,
            (int)Math.Floor(position.Z) & ~SUBCHUNK_SIDE_LENGTH_BITMASK
        );
    }
    
    
    /// <summary>
    /// World position -> position inside a chunk.
    /// Example: (36, 74, -5) -> (4, 10, 27)
    /// </summary>
    /// <returns>A new position that is relative to the chunk containing <see cref="position"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i WorldToChunkRelative(Vector3i position)
    {
        return new Vector3i(
            position.X & SUBCHUNK_SIDE_LENGTH_BITMASK,
            position.Y & SUBCHUNK_SIDE_LENGTH_BITMASK,
            position.Z & SUBCHUNK_SIDE_LENGTH_BITMASK
        );
    }
}