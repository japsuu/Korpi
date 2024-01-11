using System.Runtime.CompilerServices;
using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.Mathematics;

public static class CoordinateUtils
{
    /// <summary>
    /// World position -> chunk position.
    /// Example: (36, 74, -5) -> (32, 64, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk grid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i WorldToChunk(Vector3 position)
    {
        return new Vector3i(
            (int)System.Math.Floor(position.X) & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            (int)System.Math.Floor(position.Y) & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            (int)System.Math.Floor(position.Z) & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX
        );
    }
    
    
    /// <summary>
    /// World position -> chunk position.
    /// Example: (36, 74, -5) -> (32, 64, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk grid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i WorldToChunk(Vector3i position)
    {
        return new Vector3i(
            position.X & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            position.Y & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            position.Z & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX
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
            position.X & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            position.Z & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX
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
            (int)System.Math.Floor(position.X) & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            (int)System.Math.Floor(position.Z) & ~Constants.CHUNK_SIDE_LENGTH_MAX_INDEX
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
            position.X & Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            position.Y & Constants.CHUNK_SIDE_LENGTH_MAX_INDEX,
            position.Z & Constants.CHUNK_SIDE_LENGTH_MAX_INDEX
        );
    }
}