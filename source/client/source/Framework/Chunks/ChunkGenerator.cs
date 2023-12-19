using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public static class ChunkGenerator
{
    private const int MAX_CHUNKS_GENERATED_PER_FRAME = 8;
    
    private static readonly FastNoiseLite Noise;
    
    /// <summary>
    /// Priority queue of chunks to generate.
    /// Chunks are prioritized by their distance to the camera.
    /// </summary>
    private static readonly PriorityQueue<Vector3i, float> ChunkGenerationQueue = new();
    private static readonly HashSet<Vector3i> QueuedChunks = new();


    static ChunkGenerator()
    {
        Noise = new FastNoiseLite();
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }
    
    
    public static void QueueChunkGeneration(Vector3i chunkPos)
    {
        if (QueuedChunks.Contains(chunkPos))
            return;
        
        float distanceToPlayer = (chunkPos - PlayerEntity.LocalPlayerEntity.ViewPosition).LengthSquared;
        ChunkGenerationQueue.Enqueue(chunkPos, distanceToPlayer);
        QueuedChunks.Add(chunkPos);
    }


    public static void ProcessGenerationQueue()
    {
        int chunksGenerated = 0;
        while (chunksGenerated < MAX_CHUNKS_GENERATED_PER_FRAME && ChunkGenerationQueue.Count > 0)
        {
            Vector3i chunkPos = ChunkGenerationQueue.Dequeue();
            QueuedChunks.Remove(chunkPos);
            Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(chunkPos);
            if (chunk == null)
                continue;
            
            GenerateChunk(chunk);
            
            chunksGenerated++;
        }
    }
    
    
    private static void GenerateChunk(Chunk chunk)
    {
        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    ushort id = GetBlockIdAtPosition(new Vector3i(x + chunk.Position.X, y + chunk.Position.Y, z + chunk.Position.Z));
                    
                    chunk.SetBlockState(new Vector3i(x, y, z), BlockRegistry.GetBlock(id).GetDefaultState());
                }
            }
        }
        
        chunk.OnGenerated();
    }
    
    
    private static ushort GetBlockIdAtPosition(Vector3i blockPosition)
    {
        const int seaLevel = Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 4;
        const int terrainHeightMin = seaLevel - 16;
        const int terrainHeightMax = seaLevel + 16;
        
        float noise = Noise.GetNoise(blockPosition.X, blockPosition.Z);
        float height = noise * 0.5f + 0.5f;
        
        height = Math.Clamp(height, 0, 1);
        height = MathUtils.Lerp(terrainHeightMin, terrainHeightMax, height);
        
        if (blockPosition.Y > height)
            return 0;
        
        if (blockPosition.Y > height - 2)
            return 2;
        
        return 1;
    }
}