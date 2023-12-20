using System.Collections.Concurrent;
using BlockEngine.Client.Framework.ECS.Entities;
using ConcurrentCollections;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

/// <summary>
/// Base class for a wrapper (manager) of a <see cref="ChunkProcessorThread"/> classes.
/// Allows for easy management of a <see cref="ChunkProcessorThread"/> and provides methods to queue chunks to process.
/// </summary>
/// <typeparam name="T">The type of <see cref="ChunkProcessorThread"/> this <see cref="ChunkProcessorThreadManager{T}"/> is for</typeparam>
public abstract class ChunkProcessorThreadManager<T> : IDisposable where T : ChunkProcessorThread
{
    /// <summary>
    /// Thread that processes chunks.
    /// </summary>
    private readonly T _processorThread;


    /// <summary>
    /// How many chunks we may dispatch to the processing thread per frame.
    /// </summary>
    protected readonly int MaxChunksProcessedPerFrame;
    
    /// <summary>
    /// Priority queue from which chunks get distributed to the processing thread.
    /// Chunks are prioritized by their distance to the camera.
    /// </summary>
    protected readonly PriorityQueue<Vector3i, float> PreProcessQueue;
    
    /// <summary>
    /// HashSet to lookup chunks that are currently queued for processing.
    /// </summary>
    protected readonly ConcurrentHashSet<Vector3i> InputQueueLookup;
    
    /// <summary>
    /// Queue of chunks to process.
    /// </summary>
    protected readonly ConcurrentQueue<Vector3i> InputQueue;
    
    /// <summary>
    /// Queue of chunks that have been processed.
    /// </summary>
    protected readonly ConcurrentQueue<Vector3i> OutputQueue;


    protected ChunkProcessorThreadManager(T processorThread, int maxChunksProcessedPerFrame)
    {
        MaxChunksProcessedPerFrame = maxChunksProcessedPerFrame;
        PreProcessQueue = new PriorityQueue<Vector3i, float>();
        InputQueueLookup = new ConcurrentHashSet<Vector3i>();
        InputQueue = new ConcurrentQueue<Vector3i>();
        OutputQueue = new ConcurrentQueue<Vector3i>();
        _processorThread = processorThread;
        _processorThread.StartProcessQueues(InputQueueLookup, InputQueue, OutputQueue);
    }
    
    
    /// <summary>
    /// Adds a chunk to be processed.
    /// </summary>
    /// <param name="chunkPos">Position of the chunk to process</param>
    public void Enqueue(Vector3i chunkPos)
    {
        if (InputQueueLookup.Contains(chunkPos))
            return;
        
        PreProcessQueue.Enqueue(chunkPos, GetPriority(chunkPos));
        InputQueueLookup.Add(chunkPos);
    }
    
    
    public virtual void ProcessQueues()
    {
        int chunksQueued = 0;
        while (chunksQueued < MaxChunksProcessedPerFrame && PreProcessQueue.Count > 0)
        {
            Vector3i chunkPos = PreProcessQueue.Dequeue();
            
            Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(chunkPos);
            if (chunk == null)
            {
                InputQueueLookup.TryRemove(chunkPos);
                continue;
            }
            
            InputQueue.Enqueue(chunkPos);
            
            chunksQueued++;
        }
        
        while (OutputQueue.TryDequeue(out Vector3i chunkPos))
        {
            Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(chunkPos);
            if (chunk == null)
                continue;
            
            OnChunkProcessed(chunk);
        }
    }
    
    
    protected abstract void OnChunkProcessed(Chunk chunk);
    
    
    protected virtual float GetPriority(Vector3i chunkPos)
    {
        return (chunkPos - PlayerEntity.LocalPlayerEntity.Transform.LocalPosition).LengthSquared;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_processorThread is IDisposable processorThreadDisposable)
                processorThreadDisposable.Dispose();
            else
                throw new InvalidOperationException($"Processor thread of type {typeof(T)} does not implement {nameof(IDisposable)}!");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    
    ~ChunkProcessorThreadManager()
    {
        Dispose(false);
    }
}