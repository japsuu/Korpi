using System.Collections.Concurrent;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.ECS.Entities;
using ConcurrentCollections;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public static class ChunkGenerator
{
    private const int MAX_CHUNKS_QUEUED_PER_FRAME = 64;
    
    /// <summary>
    /// Priority queue from which chunks get distributed to the generator thread.
    /// Chunks are prioritized by their distance to the camera.
    /// </summary>
    private static readonly PriorityQueue<Vector3i, float> PreGenerationQueue;
    private static readonly ConcurrentHashSet<Vector3i> QueuedChunks;
    private static readonly ConcurrentQueue<Vector3i> GenerationQueue;
    private static readonly ChunkGeneratorThread GeneratorThread;
    
    
    static ChunkGenerator()
    {
        PreGenerationQueue = new PriorityQueue<Vector3i, float>();
        QueuedChunks = new ConcurrentHashSet<Vector3i>();
        GenerationQueue = new ConcurrentQueue<Vector3i>();
        GeneratorThread = new ChunkGeneratorThread(QueuedChunks, GenerationQueue);
    }
    
    
    /// <summary>
    /// Adds a chunk to be generated later.
    /// </summary>
    /// <param name="chunkPos">Position of the chunk to generate</param>
    public static void Queue(Vector3i chunkPos)
    {
        if (QueuedChunks.Contains(chunkPos))
            return;
        
        float distanceToPlayer = (chunkPos - PlayerEntity.LocalPlayerEntity.ViewPosition).LengthSquared;
        PreGenerationQueue.Enqueue(chunkPos, distanceToPlayer);
        QueuedChunks.Add(chunkPos);
    }


    public static void ProcessQueues()
    {
#if DEBUG
        RenderingStats.ChunksInGenerationQueue = (ulong)GenerationQueue.Count;
#endif
        int chunksQueued = 0;
        while (chunksQueued < MAX_CHUNKS_QUEUED_PER_FRAME && PreGenerationQueue.Count > 0)
        {
            Vector3i chunkPos = PreGenerationQueue.Dequeue();
            
            Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(chunkPos);
            if (chunk == null)
            {
                QueuedChunks.TryRemove(chunkPos);
                continue;
            }
            
            GenerationQueue.Enqueue(chunkPos);
            
            chunksQueued++;
        }
    }
    
    
    public static void Dispose()
    {
        GeneratorThread.Dispose();
    }
}