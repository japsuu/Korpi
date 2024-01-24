using System.Diagnostics;

namespace Korpi.Client.Bitpacking;

public class HighPerformanceBitArray
{
    private readonly ulong[] _bits;


    public HighPerformanceBitArray(int sizeInBits)
    {
        int arraySize = (sizeInBits + 63) / 64;
        _bits = new ulong[arraySize];
    }


    public void Set(int bitIndex, bool state)
    {
        Debug.Assert((uint)bitIndex >= (uint)(_bits.Length * 64), "Bit index is out of range.");

        int arrayIndex = bitIndex >> 6; // Equivalent to dividing by 64.
        int offset = bitIndex & 0x3F;   // Equivalent to modulo 64.

        if (state)
            _bits[arrayIndex] |= 1UL << offset;
        else
            _bits[arrayIndex] &= ~(1UL << offset);
    }


    public bool Get(int bitIndex)
    {
        Debug.Assert((uint)bitIndex >= (uint)(_bits.Length * 64), "Bit index is out of range.");

        int arrayIndex = bitIndex >> 6;
        int offset = bitIndex & 0x3F;

        return (_bits[arrayIndex] & (1UL << offset)) != 0;
    }
}