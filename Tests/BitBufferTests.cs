using BlockEngine.Framework.Bitpacking;

namespace Tests;

[TestFixture]
public class BitBufferTests
{
    [Test]
    public void Constructor_InitializesCorrectSizeInBits()
    {
        BitBuffer bitBuffer = new(10);
        Assert.That(bitBuffer.SizeInBits, Is.EqualTo(10));
    }

    [Test]
    public void Set_WritesBitsCorrectly()
    {
        BitBuffer bitBuffer = new(64);
        bitBuffer.Set(0, 32, 1234567890);
        Assert.That(bitBuffer.Get(0, 32), Is.EqualTo(1234567890));
    }

    [Test]
    public void Get_ReadsBitsCorrectly()
    {
        BitBuffer bitBuffer = new(32);
        bitBuffer.Set(0, 32, 1234567890);
        Assert.That(bitBuffer.Get(0, 32), Is.EqualTo(1234567890));
    }

    [Test]
    public void Set_ThrowsException_WhenBitLengthIsZero()
    {
        BitBuffer bitBuffer = new(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.Set(0, 0, 1234567890));
    }

    [Test]
    public void Get_ThrowsException_WhenBitLengthIsZero()
    {
        BitBuffer bitBuffer = new(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.Get(0, 0));
    }

    [Test]
    public void Set_ThrowsException_WhenBitIndexIsNegative()
    {
        BitBuffer bitBuffer = new(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.Set(-1, 32, 1234567890));
    }

    [Test]
    public void Get_ThrowsException_WhenBitIndexIsNegative()
    {
        BitBuffer bitBuffer = new(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.Get(-1, 32));
    }

    [Test]
    public void Set_ThrowsException_WhenBitIndexAndLengthExceedSizeInBits()
    {
        BitBuffer bitBuffer = new(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.Set(16, 32, 1234567890));
    }

    [Test]
    public void Get_ThrowsException_WhenBitIndexAndLengthExceedSizeInBits()
    {
        BitBuffer bitBuffer = new(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.Get(16, 32));
    }
}