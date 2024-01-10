using Korpi.Client.Bitpacking;
using Korpi.Client.Configuration;
using Korpi.Client.Registries;
using Korpi.Client.World.Regions.Chunks.Blocks;

namespace Korpi.Client.World.Regions.Chunks.BlockStorage;

public class BlockPalette : IBlockStorage
{
    /// <summary>
    /// How many blocks can this palette hold?
    /// Usually CHUNK_SIDE_LENGTH^3.
    /// </summary>
    private readonly int _sizeInBlocks;
    
    /// <summary>
    /// How many different (unique) blocks are in the <see cref="_palette"/>.
    /// </summary>
    private int _uniqueEntriesCount;
    
    /// <summary>
    /// How many indices are stored in the data buffer?
    /// </summary>
    private int _indexLengthInBits;
    
    /// <summary>
    /// The palette of different blocks.
    /// Contains one entry for each unique block in the chunk.
    /// </summary>
    private PaletteEntry[] _palette;
    
    /// <summary>
    /// The data buffer.
    /// Holds the indices of the blocks in <see cref="_palette"/>.
    /// </summary>
    private BitBuffer _indices;
    
    public int RenderedBlockCount { get; private set; }


    public BlockPalette()
    {
        _sizeInBlocks = Constants.CHUNK_SIDE_LENGTH_CUBED;
        _palette = new PaletteEntry[]
        {
            new(_sizeInBlocks, BlockRegistry.Air.GetDefaultState()),
            new(0, null)
        };
        _indexLengthInBits = 1;
        _uniqueEntriesCount = 1;
        _indices = new BitBuffer(_sizeInBlocks * _indexLengthInBits);    // The length is in bits, not bytes!
    }


    public void SetBlock(ChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        int index = position.Index;
        if (index < 0 || index >= _sizeInBlocks)
            throw new IndexOutOfRangeException("Tried to set blocks outside of a palette");
        
        uint paletteIndex = _indices.Get(index * _indexLengthInBits, _indexLengthInBits);

        if (_palette[paletteIndex].IsEmpty)
            oldBlock = BlockRegistry.Air.GetDefaultState();
        else
            oldBlock = _palette[paletteIndex].BlockState!.Value;
        
        // Skip if the block is the same as the old one.
        if (BlockState.EqualsNonAlloc(oldBlock, block))
            return;

        bool wasRendered = oldBlock.IsRendered;
        bool willBeRendered = block.IsRendered;
        if (wasRendered && !willBeRendered)
            RenderedBlockCount--;
        else if (!wasRendered && willBeRendered)
            RenderedBlockCount++;
    
        // Reduce the refcount of the current block-type, as an index referencing it will be overwritten.
        _palette[paletteIndex].RefCount -= 1;
    
        // See if palette has this block already, se we can use it's index.
        for(uint existingPaletteIndex = 0; existingPaletteIndex < _palette.Length; existingPaletteIndex++)
        {
            if(_palette[existingPaletteIndex].IsEmpty)
                continue;
            if (!BlockState.EqualsNonAlloc(_palette[existingPaletteIndex].BlockState!.Value, block))
                continue;
            
            // The block is in the palette already. Use the existing entry and increase it's refcount.
            _palette[existingPaletteIndex].RefCount += 1;
            
            // Write the index to the BitBuffer.
            _indices.Set(index * _indexLengthInBits, _indexLengthInBits, existingPaletteIndex);
            return;
        }
    
        // A new palette entry is needed!
        // Get the first free palette entry, possibly growing the palette!
        uint newPaletteIndex = GetNextPaletteEntry();
    
        // Overwrite the palette entry with the new block.
        _palette[newPaletteIndex].BlockState = block;
        _palette[newPaletteIndex].RefCount = 1;
        
        // Write the index to the BitBuffer.
        _indices.Set(index * _indexLengthInBits, _indexLengthInBits, newPaletteIndex);
        
        // As the entry was not previously in the palette, increase the unique entries count.
        _uniqueEntriesCount += 1;
    }
    
    
    public BlockState GetBlock(ChunkBlockPosition position)
    {
        int index = position.Index;
        uint paletteIndex = _indices.Get(index * _indexLengthInBits, _indexLengthInBits);
        
        if (_palette[paletteIndex].IsEmpty)
            throw new InvalidOperationException("Tried to get a block from an empty palette entry");
        
        return _palette[paletteIndex].BlockState!.Value;
    }


    private uint GetNextPaletteEntry()
    {
        while (true)
        {
            // See if there exist any palette entries that are empty.
            for (uint existingPaletteIndex = 0; existingPaletteIndex < _palette.Length; existingPaletteIndex++)
            {
                // If the entry is empty, return it's index so we can reuse it.
                if (_palette[existingPaletteIndex].IsEmpty)
                    return existingPaletteIndex;
            }

            // No free palette-entries - grow the palette.
            GrowPalette();
        }
    }


    /// <summary>
    /// Grows the <see cref="_palette"/> and <see cref="_indices"/> by doubling their size.
    /// </summary>
    private void GrowPalette()
    {
        // Check that we are not already at the maximum amount of unique entries.
        if (_palette.Length >= _sizeInBlocks)
            throw new InvalidOperationException("Tried to grow the palette beyond the maximum amount of unique entries");
        
        // Get the indices from the BitBuffer.
        ushort[] indices = new ushort[_sizeInBlocks];
        for(int i = 0; i < indices.Length; i++)
        {
            indices[i] = (ushort)_indices.Get(i * _indexLengthInBits, _indexLengthInBits);
        }
    
        _indexLengthInBits <<= 1;   // Double the amount of bits used to represent an index.
        int maxUniqueEntries = (int)Math.Pow(2, _indexLengthInBits);    // Calculate the new maximum amount of unique entries that can be stored in the palette.
        
        // Now because the theoretical maximum of unique entries in a chunk is CHUNK_SIDE_LENGTH_CUBED, we limit the maximum amount of unique entries to that.
        // This means that there COULD be indices that go outside of the palette, if not using a chunk size that is a power of two.
        // This is why we throw an exception early (before writing to the BitBuffer) if an index is outside of the possible range.
        if (maxUniqueEntries > _sizeInBlocks)
            maxUniqueEntries = _sizeInBlocks;
        
        // Create a new palette, and copy the old one into it.
        PaletteEntry[] newPalette = new PaletteEntry[maxUniqueEntries];
        Array.Copy(_palette, newPalette, _uniqueEntriesCount);
        _palette = newPalette;
    
        // Allocate new BitBuffer.
        _indices = new BitBuffer(_sizeInBlocks * _indexLengthInBits);
    
        // Add the old indices into the new BitBuffer.
        for(int i = 0; i < indices.Length; i++)
        {
            _indices.Set(i * _indexLengthInBits, _indexLengthInBits, indices[i]);
        }
    }


    // // Shrink the palette (and thus the BitBuffer) every now and then.
    // // You may need to apply heuristics to determine when to do this.
    // [Obsolete("May not work as intended")]
    // public void TriggerTrimExcess()
    // {
    //     // Remove old entries...
    //     for (int i = 0; i < _palette.Length; i++)
    //     {
    //         if (_palette[i].RefCount <= 0)
    //         {
    //             _uniqueEntriesCount -= 1;
    //         }
    //     }
    //
    //     // Is the _uniqueEntriesCount less than or equal to half of its closest power-of-two?
    //     if (_uniqueEntriesCount > Math.Pow(2, _uniqueEntriesCount) / 2)
    //         // NO: The palette cannot be shrunk!
    //         return;
    //
    //     // Decode all indices
    //     uint[] indices = new uint[_sizeInBlocks];
    //     for (int i = 0; i < indices.Length; i++)
    //         indices[i] = _indices.Get(i * _indexLengthInBits, _indexLengthInBits);
    //
    //     // Create new palette, halving it in size
    //     _indexLengthInBits >>= 1;
    //     PaletteEntry[] newPalette = new PaletteEntry[(int)Math.Ceiling(Math.Pow(2, _indexLengthInBits))];
    //
    //     // We gotta compress the palette entries!
    //     uint paletteCounter = 0;
    //     for (int i = 0; i < _palette.Length; i++, paletteCounter++)
    //     {
    //         PaletteEntry entry = newPalette[paletteCounter] = _palette[i];
    //
    //         // Re-encode the indices (find and replace; with limit)
    //         for (int j = 0, fc = 0; j < indices.Length && fc < entry.RefCount; j++)
    //         {
    //             if (i != indices[j])
    //                 continue;
    //             
    //             indices[j] = paletteCounter;
    //             fc += 1;
    //         }
    //     }
    //
    //     // Allocate new BitBuffer
    //     _indices = new BitBuffer(_sizeInBlocks * _indexLengthInBits);
    //
    //     // Encode the indices
    //     for (int i = 0; i < indices.Length; i++)
    //         _indices.Set(i * _indexLengthInBits, _indexLengthInBits, indices[i]);
    // }
}