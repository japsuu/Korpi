using System.Collections.Concurrent;
using BlockEngine.Client.Framework.ECS.Entities;
using ConcurrentCollections;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

/// <summary>
/// Base class for a wrapper (manager) of a <see cref="ChunkProcessorThread{T}"/> classes.
/// Allows for easy management of a <see cref="ChunkProcessorThread{T}"/> and provides methods to queue chunks to process.
/// </summary>
/// <typeparam name="TThread">The type of <see cref="ChunkProcessorThread{T}"/> this <see cref="ChunkProcessorThreadManager{T,T}"/> is for</typeparam>
/// <typeparam name="TThreadOutput">The type of output the thread will provide</typeparam>
// As C# does not support partial type inference in generic constraints, we have to specify the type of the thread output explicitly.
public abstract class ChunkProcessorThreadManager<TThread, TThreadOutput> : IDisposable where TThread : ChunkProcessorThread<TThreadOutput>
{
    /// <summary>
    /// Thread that processes chunks.
    /// </summary>
    private readonly TThread _processorThread;


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
    protected readonly ConcurrentQueue<TThreadOutput> OutputQueue;


    protected ChunkProcessorThreadManager(TThread processorThread, int maxChunksProcessedPerFrame)
    {
        MaxChunksProcessedPerFrame = maxChunksProcessedPerFrame;
        PreProcessQueue = new PriorityQueue<Vector3i, float>();
        InputQueueLookup = new ConcurrentHashSet<Vector3i>();
        InputQueue = new ConcurrentQueue<Vector3i>();
        OutputQueue = new ConcurrentQueue<TThreadOutput>();
        _processorThread = processorThread;
        _processorThread.StartProcessQueues(InputQueueLookup, InputQueue, OutputQueue);
    }


    /// <summary>
    /// Adds a chunk to be processed.
    /// </summary>
    /// <param name="chunkPos">Position of the chunk to process</param>
    public virtual void Enqueue(Vector3i chunkPos)
    {
        if (InputQueueLookup.Contains(chunkPos))
            return;
        
        if (!CanEnqueue(chunkPos))
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

            if (!World.CurrentWorld.ChunkManager.ChunkExistsAt(chunkPos))
            {
                InputQueueLookup.TryRemove(chunkPos);
                continue;
            }

            InputQueue.Enqueue(chunkPos);

            chunksQueued++;
        }

        while (OutputQueue.TryDequeue(out TThreadOutput? output))
        {
            OnChunkProcessed(output);
        }
    }


    protected abstract void OnChunkProcessed(TThreadOutput output);


    protected virtual float GetPriority(Vector3i chunkPos)
    {
        return (chunkPos - PlayerEntity.LocalPlayerEntity.Transform.LocalPosition).LengthSquared;
    }
    
    
    protected virtual bool CanEnqueue(Vector3i chunkPos) => true;


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_processorThread is IDisposable processorThreadDisposable)
                processorThreadDisposable.Dispose();
            else
                throw new InvalidOperationException($"Processor thread of type {typeof(TThread)} does not implement {nameof(IDisposable)}!");
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