using BlockEngine.Client.Framework.Bitpacking;
using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Registries;

namespace ClientTests;

public class FlatBlockStorageTests
{
    [Test]
    public void SetBlock_WithValidCoordinates_SetsBlockCorrectly()
    {
        // Arrange
        FlatBlockStorage storage = new();
        BlockState block = BlockRegistry.Air.GetDefaultState();

        // Act
        storage.SetBlock(1, 1, 1, block, out BlockState oldBlock);

        // Assert
        Assert.That(storage.GetBlock(1, 1, 1), Is.EqualTo(block));
    }

    [Test]
    public void GetBlock_WithValidCoordinates_ReturnsCorrectBlock()
    {
        // Arrange
        FlatBlockStorage storage = new();
        BlockState block = BlockRegistry.Air.GetDefaultState();
        storage.SetBlock(1, 1, 1, block, out BlockState oldBlock);

        // Act
        BlockState result = storage.GetBlock(1, 1, 1);

        // Assert
        Assert.That(result, Is.EqualTo(block));
    }

    [Test]
    public void GetBlock_WithInvalidCoordinates_ReturnsAirBlock()
    {
        // Arrange
        FlatBlockStorage storage = new();

        // Act
        BlockState result = storage.GetBlock(14, 14, 14);

        // Assert
        Assert.That(result, Is.EqualTo(BlockRegistry.Air.GetDefaultState()));
    }

    [Test]
    public void SetBlock_WithInvalidCoordinates_DoesThrowException()
    {
        // Arrange
        FlatBlockStorage storage = new();
        BlockState block = new();

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => storage.SetBlock(100, 100, 100, block, out BlockState oldBlock));
    }
}