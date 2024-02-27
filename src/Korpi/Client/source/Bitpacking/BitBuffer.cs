namespace Korpi.Client.Bitpacking;

public unsafe class BitBuffer
{
    public readonly int SizeInBits;

    private readonly byte[] _data;

    public BitBuffer(int sizeInBits)
    {
        SizeInBits = sizeInBits;
        int sizeInBytes = (int)System.Math.Ceiling(sizeInBits / 8.0);
        _data = new byte[sizeInBytes];
    }

    public void Set(int bitIndex, int bitLength, uint bits)
    {
        if (bitLength <= 0 || bitIndex < 0 || bitIndex + bitLength > SizeInBits)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Tried to write bits out of range");
        }

        fixed (byte* bPtr = _data)
        {
            // Cast the byte* to ulong*
            ulong* uPtr = (ulong*)bPtr;
            
            ArraySerializeUnsafe.Write(uPtr, bits, ref bitIndex, bitLength);
        }
    }

    public uint Get(int bitIndex, int bitLength)
    {
        if (bitLength <= 0 || bitIndex < 0 || bitIndex + bitLength > SizeInBits)
        {
            throw new ArgumentOutOfRangeException();
        }

        fixed (byte* bPtr = _data)
        {
            // Cast the byte* to ulong*
            ulong* uPtr = (ulong*)bPtr;
            
            return (uint)ArraySerializeUnsafe.Read(uPtr, ref bitIndex, bitLength);
        }
    }
}