using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Registries;

namespace Korpi.Client.World.Chunks.BlockStorage;

/// <summary>
/// Stores <see cref="BlockLayer"/>s, which in turn store the blocks in a flat array
/// </summary>
public class LayeredBlockStorage : IBlockStorage
{
    /// <summary>
    /// Stores blocks in a flat array.
    /// If the whole layer is of the same block type, the layer is stored as a single block.
    /// </summary>
    private class BlockLayer
    {
        private readonly BlockState _singleBlock;
        private bool _hasOnlyOneBlockType;
        private BlockState[]? _blocks;
    
        public int RenderedBlockCount { get; private set; }


        public BlockLayer(BlockState singleBlock)
        {
            _singleBlock = singleBlock;
            _hasOnlyOneBlockType = true;
        }


        public void SetBlock(int x, int z, BlockState block)
        {
            if (_hasOnlyOneBlockType)
            {
                if (block != _singleBlock)
                {
                    _hasOnlyOneBlockType = false;
                    _blocks = new BlockState[Constants.SUBCHUNK_SIDE_LENGTH * Constants.SUBCHUNK_SIDE_LENGTH];
                    _blocks[GetIndex(x, z)] = block;
                }
            }
            else
            {
                _blocks![GetIndex(x, z)] = block;
            }
        }


        public BlockState GetBlock(int x, int z)
        {
            if (_hasOnlyOneBlockType)
                return _singleBlock;

            return _blocks![GetIndex(x, z)];
        }
    
    
        private int GetIndex(int x, int z)
        {
            // Calculate the index in a way that minimizes cache trashing.
            return x + Constants.SUBCHUNK_SIDE_LENGTH * z;
        }
    }
    
    private readonly BlockLayer?[] _layers = new BlockLayer[Constants.SUBCHUNK_SIDE_LENGTH];
    
    public int RenderedBlockCount { get; private set; }
    
    
    public void SetBlock(SubChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        int x = position.X;
        int y = position.Y;
        int z = position.Z;
        BlockLayer? layer = _layers[y];
        if (layer == null)
        {
            layer = new BlockLayer(block);
            _layers[y] = layer;
            oldBlock = BlockRegistry.Air.GetDefaultState();
        }
        else
        {
            oldBlock = layer.GetBlock(x, z);
        }
        
        bool wasRendered = oldBlock.IsRendered;
        bool willBeRendered = block.IsRendered;
        if (wasRendered && !willBeRendered)
            RenderedBlockCount--;
        else if (!wasRendered && willBeRendered)
            RenderedBlockCount++;

        layer.SetBlock(x, z, block);
    }


    public BlockState GetBlock(SubChunkBlockPosition position)
    {
        int x = position.X;
        int y = position.Y;
        int z = position.Z;
        
        BlockLayer? layer = _layers[y];
        if (layer == null)
            return BlockRegistry.Air.GetDefaultState();

        return layer.GetBlock(x, z);
    }


    public void Clear()
    {
        for (int i = 0; i < _layers.Length; i++)
        {
            _layers[i] = null;
        }
    }
}