using BlockEngine.Framework.Blocks;

namespace BlockEngine.Framework.Bitpacking;

public struct PaletteEntry
{
    public int RefCount;
    public BlockState Block;     // WARN: Changing the state / data of a block can have unintended consequences! Find out if it's better to store just the block's ID/type instead.


    public PaletteEntry(int refCount, BlockState block)
    {
        RefCount = refCount;
        Block = block;
    }
}

public class BlockPalette
{
    /// <summary>
    /// How many blocks can this palette hold?
    /// Usually CHUNK_SIZE^3.
    /// </summary>
    private readonly int _size;
    
    /// <summary>
    /// How many different blocks are in this palette?
    /// </summary>
    private int _paletteCount;
    
    /// <summary>
    /// How many indices are stored in the data buffer?
    /// </summary>
    private int _indicesLength;
    
    /// <summary>
    /// The palette of different blocks.
    /// </summary>
    private PaletteEntry[] _palette;
    
    /// <summary>
    /// The data buffer.
    /// Holds the indices of the blocks in the palette.
    /// </summary>
    private BitBuffer _data;

    public BlockPalette(int size)
    {
        _size = size;
        _indicesLength = 1;
        _palette = new PaletteEntry[2];   // Initialize with some power of 2 value
        _data = new BitBuffer(size);    // The length is in bits, not bytes!
    }


    public void SetBlock(int index, BlockState block)
    {
        int currentPaletteIndex = _data.Get(index * _indicesLength, _indicesLength);
        PaletteEntry current = _palette[currentPaletteIndex];
    
        // Reduce the refcount of the current block-type, as it will be overwritten.
        current.RefCount -= 1;
    
        // See if we can use an existing palette entry.
        for(int existingPaletteIndex = 0; existingPaletteIndex < _paletteCount; existingPaletteIndex++)
        {
            if (!_palette[existingPaletteIndex].Block.Equals(block))
                continue;
            
            // Use the existing palette entry and increase it's refcount.
            _data.Set(index * _indicesLength, _indicesLength, existingPaletteIndex);
            _palette[existingPaletteIndex].RefCount += 1;
            return;
        }
    
        // See if we can overwrite the current palette entry?
        if(current.RefCount <= 0)
        {
            // YES, we can!
            current.Block = block;
            current.RefCount = 1;
            return;
        }
    
        // A new palette entry is needed!
        
        // Get the first free palette entry, possibly growing the palette!
        int newPaletteIndex = GetNextPaletteEntry();
    
        _palette[newPaletteIndex].RefCount = 1;
        _palette[newPaletteIndex].Block = block;
        _data.Set(index * _indicesLength, _indicesLength, newPaletteIndex);
        _paletteCount += 1;
    }
    
    
    public BlockState GetBlock(int index)
    {
        int paletteIndex = _data.Get(index * _indicesLength, _indicesLength);
        return _palette[paletteIndex].Block;
    }


    private int GetNextPaletteEntry()
    {
        while (true)
        {
            // See if we can use an existing palette entry.
            for (int existingPaletteIndex = 0; existingPaletteIndex < _paletteCount; existingPaletteIndex++)
            {
                if (_palette[existingPaletteIndex].RefCount <= 0)
                    return existingPaletteIndex;
            }

            // No free entries - grow the palette, and thus the BitBuffer.
            GrowPalette();
        }
    }


    private void GrowPalette()
    {
        // Decode the indices from the BitBuffer.
        int[] indices = new int[_size];
        for(int i = 0; i < indices.Length; i++)
        {
            indices[i] = _data.Get(i * _indicesLength, _indicesLength);
        }
    
        // Create new palette, doubling it in size.
        _indicesLength <<= 1;
        PaletteEntry[] newPalette = new PaletteEntry[(int)Math.Ceiling(Math.Pow(2, _indicesLength))];
        // TODO: Optimize the resizing by sorting the 'newPalette' based on largest refcount.
        Array.Copy(_palette, newPalette, _paletteCount);
        _palette = newPalette;
    
        // Allocate new BitBuffer.
        _data = new BitBuffer(_size * _indicesLength);
    
        // Encode the indices into the new BitBuffer.
        for(int i = 0; i < indices.Length; i++)
        {
            _data.Set(i * _indicesLength, _indicesLength, indices[i]);
        }
    }


    // Shrink the palette (and thus the BitBuffer) every now and then.
    // You may need to apply heuristics to determine when to do this.
    // TODO: Test!
    public void TrimExcess()
    {
        // Remove old entries...
        for (int i = 0; i < _palette.Length; i++)
        {
            if (_palette[i].RefCount <= 0)
            {
                _paletteCount -= 1;
            }
        }

        // Is the _paletteCount less than or equal to half of its closest power-of-two?
        
        if (_paletteCount > Math.Pow(2, _paletteCount) / 2)
            // NO: The palette cannot be shrunk!
            return;

        // Decode all indices
        int[] indices = new int[_size];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = _data.Get(i * _indicesLength, _indicesLength);

        // Create new palette, halving it in size
        _indicesLength >>= 1;
        PaletteEntry[] newPalette = new PaletteEntry[(int)Math.Ceiling(Math.Pow(2, _indicesLength))];

        // We gotta compress the palette entries!
        int paletteCounter = 0;
        for (int i = 0; i < _palette.Length; i++, paletteCounter++)
        {
            PaletteEntry entry = newPalette[paletteCounter] = _palette[i];

            // Re-encode the indices (find and replace; with limit)
            for (int j = 0, fc = 0; j < indices.Length && fc < entry.RefCount; j++)
            {
                if (i != indices[j])
                    continue;
                
                indices[j] = paletteCounter;
                fc += 1;
            }
        }

        // Allocate new BitBuffer
        _data = new BitBuffer(_size * _indicesLength);

        // Encode the indices
        for (int i = 0; i < indices.Length; i++)
            _data.Set(i * _indicesLength, _indicesLength, indices[i]);
    }
}