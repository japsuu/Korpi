using Korpi.Client.Configuration;
using Korpi.Client.World.Regions.Chunks;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Regions;

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
        int arrayIndex = y / Constants.CHUNK_SIDE_LENGTH;
        return _chunks[arrayIndex];
    }
        
    
    public bool HasChunkAtHeight(int y)
    {
        if (y < 0 || y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            return false;
        int arrayIndex = y / Constants.CHUNK_SIDE_LENGTH;
        return _chunks[arrayIndex] != null;
    }
        
    
    public Chunk? GetChunk(int i)
    {
        return _chunks[i];
    }
        
        
    public void Tick()
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Tick();
        }
    }
        
        
    public void Draw()
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Draw();
        }
    }


    public void Load()
    {
        for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
        {
            // if (Position != Vector2i.Zero)
            //     continue;
            Chunk chunk = new Chunk(new Vector3i(Position.X, i * Constants.CHUNK_SIDE_LENGTH, Position.Y));
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