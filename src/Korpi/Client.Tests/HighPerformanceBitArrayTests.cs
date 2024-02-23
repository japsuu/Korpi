using Korpi.Client.Bitpacking;

namespace ClientTests;

[TestFixture]
public class HighPerformanceBitArrayTests
{
    [Test]
    public void SetBit_SetsCorrectBitToTrue()
    {
        HighPerformanceBitArray bitArray = new(64);
        bitArray.Set(5, true);
        Assert.That(bitArray.Get(5), Is.True);
    }


    [Test]
    public void SetBit_SetsCorrectBitToFalse()
    {
        HighPerformanceBitArray bitArray = new(64);
        bitArray.Set(5, true);
        bitArray.Set(5, false);
        Assert.That(bitArray.Get(5), Is.False);
    }


    [Test]
    public void GetBit_ReturnsFalseForUnsetBit()
    {
        HighPerformanceBitArray bitArray = new(64);
        Assert.That(bitArray.Get(5), Is.False);
    }
}