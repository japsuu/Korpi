using BlockEngine.Client.World.Regions.Chunks;
using OpenTK.Mathematics;

namespace BlockEngine.Client.World.Regions;

/// <summary>
/// Vertical column of chunks.
/// </summary>
public class Region
{
    private readonly Chunk?[] _chunks;

    public readonly Vector2i Position;
        
        
    public Region(Vector2i position)
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
        
    
    public bool HasChunkAtHeight(int y)
    {
        if (y < 0 || y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            return false;
        int arrayIndex = y / Constants.CHUNK_SIZE;
        return _chunks[arrayIndex] != null;
    }
        
    
    public Chunk? GetChunk(int i)
    {
        return _chunks[i];
    }
    
    
    public void RemeshAllChunks()
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.SetMeshDirty();
        }
    }
        
        
    public void Tick()
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Tick();
        }
    }


    public void Load()
    {
        for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
        {
            // if (Position != Vector2i.Zero)
            //     continue;
            Chunk chunk = new Chunk(new Vector3i(Position.X, i * Constants.CHUNK_SIZE, Position.Y));
            chunk.Load();
            _chunks[i] = chunk;
        }
    }
        
        
    public void Unload()
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Unload();
        }
    }
}