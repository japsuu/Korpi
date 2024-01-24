using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.Rendering;
using Korpi.Client.World.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

/// <summary>
/// Vertical column of sub-chunks.
/// </summary>
public class Chunk
{
    private readonly SubChunk[] _chunks;
    private readonly ChunkHeightmap _heightmap;

    public readonly int X;
    public readonly int Z;
        
        
    public Chunk(int x, int z)
    {
        X = x;
        Z = z;
        _chunks = new SubChunk[Constants.CHUNK_HEIGHT_SUBCHUNKS];
        _heightmap = new ChunkHeightmap();
    }
        
        
    public bool ReadyToUnload() => true;
        
    
    public SubChunk GetChunkAtHeight(int y)
    {
        int arrayIndex = y / Constants.SUBCHUNK_SIDE_LENGTH;
        return _chunks[arrayIndex];
    }
        
    
    public SubChunk GetChunk(int i)
    {
        return _chunks[i];
    }
    
    
    /// <returns>The highest block at the given x and z coordinates. -1 if no blocks are found.</returns>
    public int GetHighestBlock(int x, int z) => _heightmap.GetHighestBlock(x, z);


    public void Tick()
    {
        foreach (SubChunk? chunk in _chunks)
        {
            chunk.Tick();
        }
    }
        
        
    public void Draw(RenderPass pass)
    {
        foreach (SubChunk? chunk in _chunks)
        {
            chunk.Draw(pass);
        }
    }


    public void Load()
    {
        for (int i = 0; i < Constants.CHUNK_HEIGHT_SUBCHUNKS; i++)
        {
            SubChunk chunk = new(new Vector3i(X, i * Constants.SUBCHUNK_SIDE_LENGTH, Z));
            chunk.Load();
            _chunks[i] = chunk;
        }
    }
        
        
    public void Unload()
    {
        foreach (SubChunk? chunk in _chunks)
            chunk.Unload();
    }


    public void SetBlockState(int x, int y, int z, BlockState block, out BlockState oldBlock, bool delayedMeshDirtying)
    {
        Debug.Assert(x >= 0 && x < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_HEIGHT_BLOCKS);
        Debug.Assert(z >= 0 && z < Constants.SUBCHUNK_SIDE_LENGTH);
        
        int arrayIndex = y / Constants.SUBCHUNK_SIDE_LENGTH;
        SubChunkBlockPosition pos = new(x, y % Constants.SUBCHUNK_SIDE_LENGTH, z);
        _chunks[arrayIndex].SetBlockState(pos, block, out oldBlock, delayedMeshDirtying);
        
        // Update heightmap
        if (block.IsAir)
            _heightmap.OnBlockRemoved(x, y, z);
        else
            _heightmap.OnBlockAdded(x, y, z);
    }


    public BlockState GetBlockState(int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_HEIGHT_BLOCKS);
        Debug.Assert(z >= 0 && z < Constants.SUBCHUNK_SIDE_LENGTH);
        
        int arrayIndex = y / Constants.SUBCHUNK_SIDE_LENGTH;
        SubChunkBlockPosition pos = new(x, y % Constants.SUBCHUNK_SIDE_LENGTH, z);
        return _chunks[arrayIndex].GetBlockState(pos);
    }
}