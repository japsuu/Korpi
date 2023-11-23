using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Chunks;

public class ChunkColumn
{
    private readonly Chunk?[] _chunks;

    public readonly Vector2i Position;
    
    public bool IsMeshDirty;
        
        
    public ChunkColumn(Vector2i position)
    {
        Position = position;
        _chunks = new Chunk[Constants.CHUNK_COLUMN_HEIGHT];
    }
        
        
    public bool ReadyToUnload() => true;
        
    
    public Chunk? GetChunkAtHeight(int y)
    {
        if (y < 0 || y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            return null;
        int arrayIndex = y / Constants.CHUNK_SIZE;
        return _chunks[arrayIndex];
    }
        
    
    public Chunk? GetChunk(int i)
    {
        return _chunks[i];
    }
        
        
    public void Tick(double deltaTime)
    {
        IsMeshDirty = false;
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Tick(deltaTime);
            
            if (chunk?.IsMeshDirty == true)
                IsMeshDirty = true;
        }
    }


    public void Load()
    {
        // if (Position != Vector2i.Zero)
        //     return;

        // Generate test data
        for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
        //for (int i = 0; i < 1; i++)
        {
            Chunk chunk = new Chunk();
            for (int z = 0; z < Constants.CHUNK_SIZE; z++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    if (z % 2 == 0)
                        continue;
                    
                    if ((x + 1) % 2 == 0)
                        continue;

                    chunk.SetBlockState(new Vector3i(x, 0, z), BlockRegistry.TestBlock.GetDefaultState());
                }
            }
            //chunk.SetBlockState(new Vector3i(0, 0, 0), BlockRegistry.Stone.GetDefaultState());
            //chunk.SetBlockState(new Vector3i(Constants.CHUNK_SIZE - 1, Constants.CHUNK_SIZE - 1, Constants.CHUNK_SIZE - 1), BlockRegistry.Stone.GetDefaultState());
            _chunks[i] = chunk;
        }
    }
        
        
    public void Unload()
    {
    }
}