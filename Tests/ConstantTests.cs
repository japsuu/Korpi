using BlockEngine;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace Tests;

[TestFixture]
public class ConstantsTests
{
    [Test]
    public void EngineName_IsNotNullOrEmpty()
    {
        Assert.That(string.IsNullOrEmpty(Constants.ENGINE_NAME), Is.False);
    }

    [Test]
    public void EngineVersion_IsNotNullOrEmpty()
    {
        Assert.That(string.IsNullOrEmpty(Constants.ENGINE_VERSION), Is.False);
    }

    [Test]
    public void ShaderPath_IsNotNullOrEmpty()
    {
        Assert.That(string.IsNullOrEmpty(Constants.SHADER_PATH), Is.False);
    }

    [Test]
    public void TexturePath_IsNotNullOrEmpty()
    {
        Assert.That(string.IsNullOrEmpty(Constants.TEXTURE_PATH), Is.False);
    }

    [Test]
    public void UpdateLoopFrequency_IsNotNegative()
    {
        Assert.That(Constants.UPDATE_LOOP_FREQUENCY, Is.GreaterThanOrEqualTo(0u));
    }

    [Test]
    public void ChunkSize_IsPowerOfTwo()
    {
        Assert.That(Constants.CHUNK_SIZE % 2, Is.EqualTo(0));
    }

    [Test]
    public void ChunkColumnHeight_IsPowerOfTwo()
    {
        Assert.That(Constants.CHUNK_COLUMN_HEIGHT % 2, Is.EqualTo(0));
    }

    [Test]
    public void ChunkSizeBitmask_IsCalculatedCorrectly()
    {
        Assert.That(Constants.CHUNK_SIZE_BITMASK, Is.EqualTo(Constants.CHUNK_SIZE - 1));
    }

    [Test]
    public void ChunkSizeCubed_IsCalculatedCorrectly()
    {
        Assert.That(Constants.CHUNK_SIZE_CUBED, Is.EqualTo(Constants.CHUNK_SIZE * Constants.CHUNK_SIZE * Constants.CHUNK_SIZE));
    }

    [Test]
    public void ChunkSizeLog2_IsCalculatedCorrectly()
    {
        Assert.That((int)Math.Log(Constants.CHUNK_SIZE, 2), Is.EqualTo(Constants.CHUNK_SIZE_LOG2));
    }

    [Test]
    public void ChunkSizeLog2Doubled_IsCalculatedCorrectly()
    {
        Assert.That(Constants.CHUNK_SIZE_LOG2_DOUBLED, Is.EqualTo(Constants.CHUNK_SIZE_LOG2 * 2));
    }

    [Test]
    public void ChunkColumnHeightBlocks_IsCalculatedCorrectly()
    {
        Assert.That(Constants.CHUNK_COLUMN_HEIGHT_BLOCKS, Is.EqualTo(Constants.CHUNK_COLUMN_HEIGHT * Constants.CHUNK_SIZE));
    }

    [Test]
    public void ChunkColumnLoadRadiusSquared_IsCalculatedCorrectly()
    {
        Assert.That(Constants.CHUNK_COLUMN_LOAD_RADIUS_SQUARED, Is.EqualTo(Constants.CHUNK_COLUMN_LOAD_RADIUS * Constants.CHUNK_COLUMN_LOAD_RADIUS));
    }

    [Test]
    public void ChunkColumnUnloadRadiusSquared_IsCalculatedCorrectly()
    {
        Assert.That(Constants.CHUNK_COLUMN_UNLOAD_RADIUS_SQUARED, Is.EqualTo(Constants.CHUNK_COLUMN_UNLOAD_RADIUS * Constants.CHUNK_COLUMN_UNLOAD_RADIUS));
    }

    [Test]
    public void MaxLightLevel_IsSetCorrectly()
    {
        Assert.That(Constants.MAX_LIGHT_LEVEL, Is.EqualTo(31));
    }
}