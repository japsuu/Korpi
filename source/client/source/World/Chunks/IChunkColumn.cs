using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

public interface IChunkColumn
{
    /// <summary>
    /// The position of this chunk column.
    /// </summary>
    public Vector2i Position { get; }
    
    /// <summary>
    /// Checks if all neighbouring columns of this column are generated.
    /// </summary>
    /// <param name="excludeMissingChunks">If true, chunks that are not loaded are excluded from neighbourhood checks</param>
    /// <returns>True if all neighbouring chunks are generated, false otherwise</returns>
    public bool AreAllNeighboursGenerated(bool excludeMissingChunks);


    /// <returns>The highest block at the given x and z coordinates. -1 if no blocks are found.</returns>
    public int GetHighestBlock(int x, int z);
}