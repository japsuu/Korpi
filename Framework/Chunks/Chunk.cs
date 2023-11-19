using BlockEngine.Framework.Bitpacking;
using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Meshing;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Chunks;

public class Chunk
{
    private readonly BlockPalette _blocks;

    public bool IsMeshDirty;


    public Chunk(BlockPalette data)
    {
        _blocks = data;
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public void SetBlockState(Vector3i position, BlockState block)
    {
        // Use (z << (W + H)) + (y << W) + x to index into a 3D array stored as a 1D array.
        // So, in this formula, "W" and "H" are the logarithms (base 2) of the dimensions of the array in the x and y directions, respectively.
        // In other words, if the width and height of the 3D array are both 2^N, then "W" and "H" would be equal to "N."
        int index = (position.Z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (position.Y << Constants.CHUNK_SIZE_LOG2) + position.X;
        _blocks.SetBlock(index, block);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(Vector3i position)
    {
        // Use (z << (W + H)) + (y << W) + x to index into a 3D array stored as a 1D array.
        // So, in this formula, "W" and "H" are the logarithms (base 2) of the dimensions of the array in the x and y directions, respectively.
        // In other words, if the width and height of the 3D array are both 2^N, then "W" and "H" would be equal to "N."
        int index = (position.Z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (position.Y << Constants.CHUNK_SIZE_LOG2) + position.X;
        return _blocks.GetBlock(index);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(int x, int y, int z)
    {
        // Use (z << (W + H)) + (y << W) + x to index into a 3D array stored as a 1D array.
        // So, in this formula, "W" and "H" are the logarithms (base 2) of the dimensions of the array in the x and y directions, respectively.
        // In other words, if the width and height of the 3D array are both 2^N, then "W" and "H" would be equal to "N."
        int index = (z << Constants.CHUNK_SIZE_LOG2_DOUBLED) + (y << Constants.CHUNK_SIZE_LOG2) + x;
        return _blocks.GetBlock(index);
    }


    public void Tick(double deltaTime)
    {
    }


    public void CacheAsCenter(MeshingDataCache meshingDataCache)
    {
        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    // Offset by one block in each direction to account for the border
                    meshingDataCache.SetData(x + 1, y + 1, z + 1, GetBlockState(x, y, z));
                }
            }
        }
    }


    /// <summary>
    /// Used to get a slice of block data from this chunk.
    /// The size of the slice depends on which neighbouring chunk is asking for the data.
    /// Alternative method would be to query the World.GetBlockData method for each block in the cache, but this would result in cache misses.
    /// </summary>
    /// <param name="cache">Cache to fill with data</param>
    /// <param name="position">Position of this chunk relative to the requesting chunk</param>
    public void CacheAsNeighbour(MeshingDataCache cache, NeighbouringChunkPosition position)
    {
        switch (position)
        {
            // Corners
            case NeighbouringChunkPosition.CornerNorthEastUp:
                cache.SetData(cache.BorderBlockIndex, cache.BorderBlockIndex, cache.BorderBlockIndex, GetBlockState(0, 0, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthEastUp:
                cache.SetData(0, cache.BorderBlockIndex, cache.BorderBlockIndex, GetBlockState(Constants.CHUNK_SIZE_BITMASK, 0, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthWestUp:
                cache.SetData(0, cache.BorderBlockIndex, 0, GetBlockState(Constants.CHUNK_SIZE_BITMASK, 0, Constants.CHUNK_SIZE_BITMASK));
                break;
            case NeighbouringChunkPosition.CornerNorthWestUp:
                cache.SetData(cache.BorderBlockIndex, cache.BorderBlockIndex, 0, GetBlockState(0, 0, Constants.CHUNK_SIZE_BITMASK));
                break;
            case NeighbouringChunkPosition.CornerNorthEastDown:
                cache.SetData(cache.BorderBlockIndex, 0, cache.BorderBlockIndex, GetBlockState(0, Constants.CHUNK_SIZE_BITMASK, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthEastDown:
                cache.SetData(0, 0, cache.BorderBlockIndex, GetBlockState(Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK, 0));
                break;
            case NeighbouringChunkPosition.CornerSouthWestDown:
                cache.SetData(0, 0, 0, GetBlockState(Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK));
                break;
            case NeighbouringChunkPosition.CornerNorthWestDown:
                cache.SetData(cache.BorderBlockIndex, 0, 0, GetBlockState(0, Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK));
                break;
            
            // Edges
            case NeighbouringChunkPosition.EdgeNorthUp:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(cache.BorderBlockIndex, cache.BorderBlockIndex, z + 1, GetBlockState(0, 0, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeEastUp:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, cache.BorderBlockIndex, cache.BorderBlockIndex, GetBlockState(x, 0, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthUp:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(0, cache.BorderBlockIndex, z + 1, GetBlockState(Constants.CHUNK_SIZE_BITMASK, 0, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeWestUp:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, cache.BorderBlockIndex, 0, GetBlockState(x, 0, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            case NeighbouringChunkPosition.EdgeNorthDown:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(cache.BorderBlockIndex, 0, z + 1, GetBlockState(0, Constants.CHUNK_SIZE_BITMASK, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeEastDown:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, 0, cache.BorderBlockIndex, GetBlockState(x, Constants.CHUNK_SIZE_BITMASK, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthDown:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    cache.SetData(0, 0, z + 1, GetBlockState(Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK, z));
                }
                break;
            case NeighbouringChunkPosition.EdgeWestDown:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    cache.SetData(x + 1, 0, 0, GetBlockState(x, Constants.CHUNK_SIZE_BITMASK, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            case NeighbouringChunkPosition.EdgeNorthEast:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(cache.BorderBlockIndex, y + 1, cache.BorderBlockIndex, GetBlockState(0, y, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthEast:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(0, y + 1, cache.BorderBlockIndex, GetBlockState(Constants.CHUNK_SIZE_BITMASK, y, 0));
                }
                break;
            case NeighbouringChunkPosition.EdgeSouthWest:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(0, y + 1, 0, GetBlockState(Constants.CHUNK_SIZE_BITMASK, y, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            case NeighbouringChunkPosition.EdgeNorthWest:
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    cache.SetData(cache.BorderBlockIndex, y + 1, 0, GetBlockState(0, y, Constants.CHUNK_SIZE_BITMASK));
                }
                break;
            
            // Faces
            case NeighbouringChunkPosition.FaceNorth:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(cache.BorderBlockIndex, y + 1, z + 1, GetBlockState(0, y, z));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceEast:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(x + 1, y + 1, cache.BorderBlockIndex, GetBlockState(x, y, 0));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceSouth:
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(0, y + 1, z + 1, GetBlockState(Constants.CHUNK_SIZE_BITMASK, y, z));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceWest:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    {
                        cache.SetData(x + 1, y + 1, 0, GetBlockState(x, y, Constants.CHUNK_SIZE_BITMASK));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceUp:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        cache.SetData(x + 1, cache.BorderBlockIndex, z + 1, GetBlockState(x, 0, z));
                    }
                }
                break;
            case NeighbouringChunkPosition.FaceDown:
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        cache.SetData(x + 1, 0, z + 1, GetBlockState(x, Constants.CHUNK_SIZE_BITMASK, z));
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(position), position, null);
        }
    }
}