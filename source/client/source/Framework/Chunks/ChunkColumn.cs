using BlockEngine.Client.Framework.Registries;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkColumn
{
    private static readonly Random Rng = new();
    
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
        
        // if (Position.X > 1 * Constants.CHUNK_SIZE || Position.X < -1 * Constants.CHUNK_SIZE || Position.Y > 1 * Constants.CHUNK_SIZE || Position.Y < -1 * Constants.CHUNK_SIZE)
        //     return;
        
        if (Position.X > 2 * Constants.CHUNK_SIZE || Position.X < -2 * Constants.CHUNK_SIZE || Position.Y > 2 * Constants.CHUNK_SIZE || Position.Y < -2 * Constants.CHUNK_SIZE)
            return;

        // Generate test data
        //for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
        for (int i = 0; i < 1; i++)
        {
            Chunk chunk = new Chunk();
            for (int z = 0; z < Constants.CHUNK_SIZE; z++)
            {
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                    {
                        bool isChecker = (x + y + z) % 2 == 0;
                        if (isChecker)
                            continue;

                        ushort id = (ushort)Rng.Next(1, BlockRegistry.GetBlockCount());
                        if (Rng.NextDouble() < 0.8f)
                        {
                            id = 0;
                        }
                        chunk.SetBlockState(new Vector3i(x, y, z), BlockRegistry.GetBlock(id).GetDefaultState());
                    }
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