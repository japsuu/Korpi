using BlockEngine.Client.Framework.Bitpacking;
using BlockEngine.Client.Framework.Blocks;

namespace ClientTests;

[TestFixture]
public class BlockPaletteTests
{
    private BlockPalette _blockPalette = null!;

    [SetUp]
    public void SetUp()
    {
        _blockPalette = new BlockPalette();
    }

    [Test]
    public void SetBlock_ValidCoordinates_SetsBlockSuccessfully()
    {
        BlockState blockState = new();

        _blockPalette.SetBlock(1, 1, 1, blockState, out BlockState oldBlock);

        Assert.That(_blockPalette.GetBlock(1, 1, 1), Is.EqualTo(blockState));
    }

    [Test]
    public void SetBlock_InvalidCoordinates_ThrowsArgumentOutOfRangeException()
    {
        BlockState blockState = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => _blockPalette.SetBlock(-1, -1, -1, blockState, out BlockState oldBlock));
    }

    [Test]
    public void GetBlock_ValidCoordinates_ReturnsBlockState()
    {
        BlockState blockState = new();
        _blockPalette.SetBlock(1, 1, 1, blockState, out BlockState oldBlock);

        BlockState result = _blockPalette.GetBlock(1, 1, 1);

        Assert.That(result, Is.EqualTo(blockState));
    }

    [Test]
    public void GetBlock_InvalidCoordinates_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _blockPalette.GetBlock(-1, -1, -1));
    }

    /*[Test]
    public void TrimExcess_WhenCalled_ShouldReducePaletteSize()
    {
        int originalSize = _blockPalette.GetPaletteSize();
        // Set blocks to increase palette size
        for (int i = 0; i < Constants.CHUNK_SIZE; i++)
        {
            for (int j = 0; j < Constants.CHUNK_SIZE; j++)
            {
                for (int k = 0; k < Constants.CHUNK_SIZE; k++)
                {
                    _blockPalette.SetBlock(i, j, k, new BlockState());
                }
            }
        }

        _blockPalette.TriggerTrimExcess();

        // Assert that palette size is reduced
        Assert.That(_blockPalette.GetPaletteSize(), Is.LessThan(originalSize));
    }*/
    
    
    [Test]
    public void GrowPalette_WhenCalled_ShouldIncreasePaletteSize()
    {
        // Arrange
        int initialPaletteSize = _blockPalette.GetPaletteSize();

        // Act
        _blockPalette.TriggerGrowPalette();

        // Assert
        int finalPaletteSize = _blockPalette.GetPaletteSize();
        Assert.That(finalPaletteSize, Is.GreaterThan(initialPaletteSize));
    }
}