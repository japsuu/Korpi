using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Chunks;
using OpenTK.Mathematics;

namespace ClientTests;

public class ChunkTests
{
    [Test]
    public void SetBlockState_UpdatesBlockStateAndSetsMeshDirty()
    {
        // Arrange
        Chunk chunk = new();
        Vector3i position = new(1, 1, 1);
        BlockState blockState = new();

        // Act
        chunk.SetBlockState(position, blockState);

        // Assert
        Assert.IsTrue(chunk.IsMeshDirty);
    }

    [Test]
    public void GetBlockState_ReturnsCorrectBlockState()
    {
        // Arrange
        Chunk chunk = new();
        Vector3i position = new(1, 1, 1);
        BlockState blockState = new();
        chunk.SetBlockState(position, blockState);

        // Act
        BlockState result = chunk.GetBlockState(position.X, position.Y, position.Z);

        // Assert
        Assert.That(result, Is.EqualTo(blockState));
    }
}