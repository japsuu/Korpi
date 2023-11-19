using BlockEngine.Framework.Meshing;
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
    }
        
        
    public void Unload()
    {
    }
}