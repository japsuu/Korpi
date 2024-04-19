using Korpi.Client.Blocks;
using Korpi.Client.World.Chunks;
using OpenTK.Mathematics;

namespace Client.Tests;

public class ChunkTests
{
    private class DummyChunkColumn : IChunkColumn
    {
        public Vector2i Position => Vector2i.Zero;

        public bool AreAllNeighboursGenerated(bool excludeMissingChunks) => true;

        public int GetHighestBlock(int x, int z) => 0;
    }
    
    [Test]
    public void GetBlockState_ReturnsCorrectBlockState()
    {
        // Arrange
        Chunk chunk = new(new DummyChunkColumn(), 0);
        ChunkBlockPosition position = new(1, 1, 1);
        BlockState blockState = new();
        chunk.SetBlockState(position, blockState, out _, false);

        // Act
        BlockState result = chunk.GetBlockState(new ChunkBlockPosition(position.X, position.Y, position.Z));

        // Assert
        Assert.That(result, Is.EqualTo(blockState));
    }
}