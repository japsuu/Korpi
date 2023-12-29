using OpenTK.Mathematics;

namespace BlockEngine.Client.Utils;

public class CoordinateConversions
{
    /// <summary>
    /// World position -> chunk position.
    /// Example: (36, 74, -5) -> (32, 64, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk grid</returns>
    public static Vector3i WorldToChunk(Vector3 position)
    {
        return new Vector3i(
            (int)Math.Floor(position.X) & ~Constants.CHUNK_SIZE_BITMASK,
            (int)Math.Floor(position.Y) & ~Constants.CHUNK_SIZE_BITMASK,
            (int)Math.Floor(position.Z) & ~Constants.CHUNK_SIZE_BITMASK
        );
    }
    
    
    /// <summary>
    /// World position -> chunk position.
    /// Example: (36, 74, -5) -> (32, 64, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk grid</returns>
    public static Vector3i WorldToChunk(Vector3i position)
    {
        return new Vector3i(
            position.X & ~Constants.CHUNK_SIZE_BITMASK,
            position.Y & ~Constants.CHUNK_SIZE_BITMASK,
            position.Z & ~Constants.CHUNK_SIZE_BITMASK
        );
    }
        
    
    /// <summary>
    /// World position -> chunk column position.
    /// Example: (36, 74, -5) -> (32, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk column grid</returns>
    public static Vector2i WorldToColumn(Vector3i position)
    {
        return new Vector2i(
            position.X & ~Constants.CHUNK_SIZE_BITMASK,
            position.Z & ~Constants.CHUNK_SIZE_BITMASK
        );
    }
        
    
    /// <summary>
    /// World position -> chunk column position.
    /// Example: (36, 74, -5) -> (32, -32)
    /// </summary>
    /// <returns>A new position that is relative to the chunk column grid</returns>
    public static Vector2i WorldToColumn(Vector3 position)
    {
        return new Vector2i(
            (int)Math.Floor(position.X) & ~Constants.CHUNK_SIZE_BITMASK,
            (int)Math.Floor(position.Z) & ~Constants.CHUNK_SIZE_BITMASK
        );
    }
    
    
    /// <summary>
    /// World position -> position inside a chunk.
    /// Example: (36, 74, -5) -> (4, 10, 27)
    /// </summary>
    /// <returns>A new position that is relative to the chunk containing <see cref="position"/></returns>
    public static Vector3i WorldToChunkRelative(Vector3i position)
    {
        return new Vector3i(
            position.X & Constants.CHUNK_SIZE_BITMASK,
            position.Y & Constants.CHUNK_SIZE_BITMASK,
            position.Z & Constants.CHUNK_SIZE_BITMASK
        );
    }
}