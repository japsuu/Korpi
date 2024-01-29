using Korpi.Client.Blocks;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;
using Korpi.Client.World.Chunks.BlockStorage;

namespace ClientTests.BlockStorageTests;

[TestFixture]
public abstract class BlockStorageTest<T> where T : IBlockStorage, new()
{
    [Test]
    public void SetBlock_WithValidCoordinates_SetsAndGetsCorrectBlock()
    {
        // Arrange
        T storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Opaque, null).GetDefaultState();

        // Act
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState _);

        // Assert
        Assert.That(storage.GetBlock(new ChunkBlockPosition(1, 1, 1)), Is.EqualTo(block));
    }
    
    [Test]
    public void SetBlock_WithValidCoordinates_ReplacesDefaultAir()
    {
        // Arrange
        T storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Opaque, null).GetDefaultState();

        // Act
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState oldBlock);

        // Assert
        Assert.That(oldBlock, Is.EqualTo(BlockRegistry.Air.GetDefaultState()));
    }

    [Test]
    public void GetBlock_WithInvalidCoordinates_DoesThrowException()
    {
        // Arrange
        T storage = new();

        // Act & Assert
        Assert.Catch(() => storage.GetBlock(new ChunkBlockPosition(100, 100, -100)));
    }
    
    [Test]
    public void SetBlock_WithInvalidCoordinates_DoesThrowException()
    {
        // Arrange
        T storage = new();
        BlockState block = new();

        // Act & Assert
        Assert.Catch(() => storage.SetBlock(new ChunkBlockPosition(100, 100, -100), block, out BlockState _));
    }

    [Test]
    public void SetBlock_AddRenderedBlock_IncreasesRenderedBlockCount()
    {
        // Arrange
        T storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Opaque, null).GetDefaultState();

        // Act
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState _);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(1));
    }

    [Test]
    public void SetBlock_RemoveRenderedBlock_DecreasesRenderedBlockCount()
    {
        // Arrange
        T storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Opaque, null).GetDefaultState();
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState _);

        // Act
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), BlockRegistry.Air.GetDefaultState(), out BlockState _);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(0));
    }

    [Test]
    public void SetBlock_ReplaceRenderedBlock_KeepsRenderedBlockCount()
    {
        // Arrange
        T storage = new();
        BlockState block = new Block(1, "test1", BlockRenderType.Opaque, null).GetDefaultState();
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState _);
        block = new Block(2, "test2", BlockRenderType.Opaque, null).GetDefaultState();

        // Act
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState _);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(1));
    }

    [Test]
    public void SetBlock_WithValidCoordinatesAndNonRenderedBlock_DecreasesRenderedBlockCount()
    {
        // Arrange
        T storage = new();
        BlockState block = new Block(1, "test", BlockRenderType.Opaque, null).GetDefaultState();
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block, out BlockState _);
        BlockState block2 = new Block(2, "test", BlockRenderType.None, null).GetDefaultState();

        // Act
        storage.SetBlock(new ChunkBlockPosition(1, 1, 1), block2, out BlockState _);

        // Assert
        Assert.That(storage.RenderedBlockCount, Is.EqualTo(0));
    }
}