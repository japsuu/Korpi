using BlockEngine.Utils;

namespace BlockEngine.Framework.Chunks;

public class ChunkColumn
{
    private readonly World _world;
    private readonly Chunk?[] _chunks;
        
        
    public ChunkColumn(World world)
    {
        _world = world;
        _chunks = new Chunk[Constants.CHUNK_COLUMN_HEIGHT];
    }
        
        
    public bool ReadyToUnload() => true;
        
    
    public Chunk? GetChunkAtHeight(int y)
    {
        int arrayIndex = y / Constants.CHUNK_SIZE;
        return _chunks[arrayIndex];
    }
        
        
    public void Tick(double deltaTime)
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Tick(deltaTime);
        }
    }


    public void Load()
    {
    }
        
        
    public void Unload()
    {
    }
}