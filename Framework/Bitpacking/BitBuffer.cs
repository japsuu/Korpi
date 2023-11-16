namespace BlockEngine.Framework.Bitpacking;

public unsafe class BitBuffer
{
    public readonly int SizeInBits;

    private readonly byte[] _data;

    public BitBuffer(int sizeInBits)
    {
        SizeInBits = sizeInBits;
        int sizeInBytes = (int)Math.Ceiling(sizeInBits / 8.0);
        _data = new byte[sizeInBytes];
    }

    public void Set(int bitIndex, int bitLength, int bits)
    {
        if (bitLength <= 0 || bitIndex < 0 || bitIndex + bitLength > SizeInBits)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Tried to write bits out of range");
        }

        int byteIndex = bitIndex / 8;
        int bitOffset = bitIndex % 8;

        fixed (byte* dataPtr = _data)
        {
            uint* uintPtr = (uint*)(dataPtr + byteIndex);

            int remainingBits = bitLength;
            while (remainingBits > 0)
            {
                int bitsToWrite = Math.Min(32 - bitOffset, remainingBits);
                uint mask = (uint)((1 << bitsToWrite) - 1) << (32 - bitOffset - bitsToWrite);

                *uintPtr = (*uintPtr & ~mask) | ((uint)bits << (32 - bitOffset - bitsToWrite));

                bitOffset = (bitOffset + bitsToWrite) % 8;
                remainingBits -= bitsToWrite;
                uintPtr++;
            }
        }
    }

    public int Get(int bitIndex, int bitLength)
    {
        if (bitLength <= 0 || bitIndex < 0 || bitIndex + bitLength > SizeInBits)
        {
            throw new ArgumentOutOfRangeException();
        }

        int byteIndex = bitIndex / 8;
        int bitOffset = bitIndex % 8;

        int result = 0;
        fixed (byte* dataPtr = _data)
        {
            uint* uintPtr = (uint*)(dataPtr + byteIndex);

            int remainingBits = bitLength;
            while (remainingBits > 0)
            {
                int bitsToRead = Math.Min(32 - bitOffset, remainingBits);
                uint mask = (uint)((1 << bitsToRead) - 1) << (32 - bitOffset - bitsToRead);

                result |= (int)((*uintPtr & mask) >> (32 - bitOffset - bitsToRead));

                bitOffset = (bitOffset + bitsToRead) % 8;
                remainingBits -= bitsToRead;
                uintPtr++;
            }
        }

        return result;
    }
}