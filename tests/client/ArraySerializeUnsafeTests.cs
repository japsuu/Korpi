
using BlockEngine.Client.Bitpacking;

namespace ClientTests;

[TestFixture]
public unsafe class ArraySerializeUnsafeTests
{
    private static ulong* CreateBuffer()
    {
        ulong[] buffer = new ulong[10];
        fixed (ulong* ptr = buffer)
        {
            return ptr;
        }
    }

    [Test]
    public void WriteSigned_WritesCorrectValueToBuffer()
    {
        int bitposition = 0;
        const int bits = 32;
        const int value = -12345678;
        ulong* buffer = CreateBuffer();

        ArraySerializeUnsafe.WriteSigned(buffer, value, ref bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.ReadSigned(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void AppendSigned_AppendsCorrectValueToBuffer()
    {
        int bitposition = 0;
        int bits = 32;
        int value = -12345678;
        ulong* buffer = CreateBuffer();

        ArraySerializeUnsafe.AppendSigned(buffer, value, ref bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.ReadSigned(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void AddSigned_AddsCorrectValueToBuffer()
    {
        int bitposition = 0;
        int bits = 32;
        int value = -12345678;
        ulong* buffer = CreateBuffer();

        value.AddSigned(buffer, ref bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.ReadSigned(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void InjectSigned_InjectsCorrectValueToBuffer()
    {
        int bitposition = 0;
        int bits = 32;
        int value = -12345678;
        ulong* buffer = CreateBuffer();

        value.InjectSigned(buffer, ref bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.ReadSigned(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void PokeSigned_PokesCorrectValueToBuffer()
    {
        int bitposition = 0;
        int bits = 32;
        int value = -12345678;
        ulong* buffer = CreateBuffer();

        value.PokeSigned(buffer, bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.ReadSigned(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void ReadSigned_ReadsCorrectValueFromBuffer()
    {
        int bitposition = 0;
        int bits = 32;
        int value = -12345678;
        ulong* buffer = CreateBuffer();

        ArraySerializeUnsafe.WriteSigned(buffer, value, ref bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.ReadSigned(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void PeekSigned_PeeksCorrectValueFromBuffer()
    {
        int bitposition = 0;
        int bits = 32;
        int value = -12345678;
        ulong* buffer = CreateBuffer();

        ArraySerializeUnsafe.WriteSigned(buffer, value, ref bitposition, bits);

        bitposition = 0;
        int readValue = ArraySerializeUnsafe.PeekSigned(buffer, bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }
    
    [Test]
    public void Write_WritesCorrectValueToBuffer()
    {
        int bitposition = 0;
        const int bits = 32;
        const ulong value = 12345678;
        ulong* buffer = CreateBuffer();

        ArraySerializeUnsafe.Write(buffer, value, ref bitposition, bits);

        bitposition = 0;
        ulong readValue = ArraySerializeUnsafe.Read(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void Read_ReadsCorrectValueFromBuffer()
    {
        int bitposition = 0;
        const int bits = 32;
        const ulong value = 12345678;
        ulong* buffer = CreateBuffer();

        ArraySerializeUnsafe.Write(buffer, value, ref bitposition, bits);

        bitposition = 0;
        ulong readValue = ArraySerializeUnsafe.Read(buffer, ref bitposition, bits);

        Assert.That(readValue, Is.EqualTo(value));
    }
}