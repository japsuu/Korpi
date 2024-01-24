using System.Diagnostics;
using System.Text;

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
#if DEBUG
        if (bitIndex < 0 || bitIndex >= _bits.Length * 64)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");
#endif
        int arrayIndex = bitIndex >> 6; // Equivalent to dividing by 64.
        int offset = bitIndex & 63;   // Equivalent to modulo 64.

        if (state)
            _bits[arrayIndex] |= 1UL << offset;
        else
            _bits[arrayIndex] &= ~(1UL << offset);
    }


    public bool Get(int bitIndex)
    {
#if DEBUG
        if (bitIndex < 0 || bitIndex >= _bits.Length * 64)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");
#endif
        int arrayIndex = bitIndex >> 6;
        int offset = bitIndex & 63;

        return (_bits[arrayIndex] & (1UL << offset)) != 0;
    }

    public override string ToString()
    {
        string name = GetType().Name;
        StringBuilder sb = new(name.Length + _bits.Length * 64);
        sb.Append(name);
        foreach (ulong u in _bits)
        {
            for (int j = 0; j < 64; j++)
            {
                bool bit = (u & (1UL << j)) != 0;
                sb.Append(bit ? '1' : '0');
            }
        }
        return sb.ToString();
    }
}