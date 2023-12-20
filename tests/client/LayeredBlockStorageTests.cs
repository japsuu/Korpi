using BlockEngine.Client.Framework.Bitpacking;
using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Registries;

namespace ClientTests;

public class LayeredBlockStorageTests
{
    [Test]
    public void SetBlock_WithValidCoordinates_SetsCorrectBlock()
    {
        // Arrange
        LayeredBlockStorage storage = new();
        BlockState block = BlockRegistry.Air.GetDefaultState();

        // Act
        storage.SetBlock(1, 1, 1, block, out BlockState oldBlock);

        // Assert
        Assert.That(storage.GetBlock(1, 1, 1), Is.EqualTo(block));
    }

    [Test]
    public void GetBlock_WithValidCoordinates_GetsCorrectBlock()
    {
        // Arrange
        LayeredBlockStorage storage = new();
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
        LayeredBlockStorage storage = new();

        // Act
        BlockState result = storage.GetBlock(14, 14, 14);

        // Assert
        Assert.That(result, Is.EqualTo(BlockRegistry.Air.GetDefaultState()));
    }

    [Test]
    public void SetBlock_WithValidCoordinates_ChangesRenderedBlockCount()
    {
        // Arrange
        LayeredBlockStorage storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Normal, null).GetDefaultState();

        // Act
        storage.SetBlock(1, 1, 1, block, out BlockState oldBlock);
        
        Console.WriteLine(block.IsRendered);
        Console.WriteLine(oldBlock.IsRendered);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(1));
    }

    [Test]
    public void SetBlock_WithValidCoordinatesAndRenderedBlock_DoesNotChangeRenderedBlockCount()
    {
        // Arrange
        LayeredBlockStorage storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Normal, null).GetDefaultState();
        storage.SetBlock(1, 1, 1, block, out BlockState oldBlock);

        // Act
        storage.SetBlock(1, 1, 1, block, out oldBlock);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(1));
    }

    [Test]
    public void SetBlock_WithValidCoordinatesAndNonRenderedBlock_DecreasesRenderedBlockCount()
    {
        // Arrange
        LayeredBlockStorage storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Normal, null).GetDefaultState();
        storage.SetBlock(1, 1, 1, block, out BlockState oldBlock);
        BlockState block2 = new Block(2, "test", BlockRenderType.None, null).GetDefaultState();

        // Act
        storage.SetBlock(1, 1, 1, block2, out oldBlock);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(0));
    }
}