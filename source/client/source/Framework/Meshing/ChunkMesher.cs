using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.ECS.Entities;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

public static class ChunkMesher : ChunkProcessorThreadManager<ChunkMesherThread>
{
    private const int MAX_CHUNKS_QUEUED_PER_FRAME = 64;
    
    /// <summary>
    /// Priority queue of chunks to mesh.
    /// Chunks are prioritized by their distance to the camera.
    /// </summary>
    private static readonly PriorityQueue<Vector3i, float> ChunkMeshingQueue;
    private static readonly HashSet<Vector3i> QueuedChunks;


    static ChunkMesher()
    {
        ChunkMeshingQueue = new PriorityQueue<Vector3i, float>();
        QueuedChunks = new HashSet<Vector3i>();
    }


    public static void Enqueue(Vector3i chunkOriginPos)
    {
        if (QueuedChunks.Contains(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already queued!");

        if (ChunkRendererStorage.ContainsRenderer(chunkOriginPos))
            ChunkRendererStorage.InvalidateRenderer(chunkOriginPos);

        float distanceToPlayer = (chunkOriginPos - PlayerEntity.LocalPlayerEntity.Transform.LocalPosition).LengthSquared;
        ChunkMeshingQueue.Enqueue(chunkOriginPos, distanceToPlayer);
        QueuedChunks.Add(chunkOriginPos);
    }


    public static void ProcessQueues()
    {
        RenderingStats.ChunksInMeshingQueue = (ulong)ChunkMeshingQueue.Count;
        RenderingStats.StartMeshing();
        
        int chunksMeshed = 0;
        while (chunksMeshed < MAX_CHUNKS_QUEUED_PER_FRAME && ChunkMeshingQueue.Count > 0)
        {
            Vector3i chunkPos = ChunkMeshingQueue.Dequeue();
            QueuedChunks.Remove(chunkPos);
            
            // Check if the chunk is still loaded
            if (!World.CurrentWorld.ChunkManager.IsChunkLoaded(chunkPos))
                continue;
            
            RenderingStats.StartChunkMeshing();
            ChunkRenderer mesh = GenerateMesh(chunkPos);
            ChunkRendererStorage.AddRenderer(chunkPos, mesh);
            chunksMeshed++;
            RenderingStats.StopChunkMeshing();
        }
        
        RenderingStats.StopMeshing(chunksMeshed);
    }


    
}