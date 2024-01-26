using Korpi.Client.World;
using Korpi.Client.World.Chunks;
using Korpi.Client.World.Chunks.Blocks;
using OpenTK.Mathematics;

namespace ClientTests;

public class SubChunkTests
{
    [Test]
    public void GetBlockState_ReturnsCorrectBlockState()
    {
        // Arrange
        SubChunk subChunk = new(new Vector3i(0, 0, 0));
        SubChunkBlockPosition position = new(1, 1, 1);
        BlockState blockState = new();
        subChunk.SetBlockState(position, blockState, out _, false);

        // Act
        BlockState result = subChunk.GetBlockState(new SubChunkBlockPosition(position.X, position.Y, position.Z));

        // Assert
        Assert.That(result, Is.EqualTo(blockState));
    }
}