using System.Collections.Concurrent;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Chunks;
using ConcurrentCollections;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Threading;

/// <summary>
/// Base class for a thread that processes chunks and provides some kind of output.
/// </summary>
/// <typeparam name="T">The type of data to output. MUST BE THREAD SAFE!</typeparam>
public abstract class ChunkProcessorThread<T> : IDisposable
{
    private volatile bool _shouldStop;
    
    private readonly Thread _thread;
    private ConcurrentHashSet<Vector3i>? _inputQueueLookup;
    private ConcurrentQueue<Vector3i>? _inputQueue;
    private ConcurrentQueue<T>? _outputQueue;


    protected ChunkProcessorThread()
    {
        _thread = new Thread(ProcessQueues);
        _thread.Name = GetType().FullName;
    }


    public void StartProcessQueues(ConcurrentHashSet<Vector3i> inputQueueLookup, ConcurrentQueue<Vector3i> inputQueue, ConcurrentQueue<T> outputQueue)
    {
        _inputQueueLookup = inputQueueLookup;
        _inputQueue = inputQueue;
        _outputQueue = outputQueue;
        
        _thread.Start();
    }


    private void ProcessQueues()
    {
        if (_inputQueueLookup == null || _inputQueue == null || _outputQueue == null)
            throw new InvalidOperationException($"{GetType().FullName} was not bound to queues when started!");
        
        InitializeThread();
        
        while (!_shouldStop)
        {
            if (!_inputQueue.TryDequeue(out Vector3i chunkPos))
                continue;
            
            Chunk? chunk = GameWorld.CurrentGameWorld.ChunkManager.GetChunkAt(chunkPos);
            if (chunk == null)
                continue;
                
            T output = ProcessChunk(chunk);
            _inputQueueLookup.TryRemove(chunkPos);
            _outputQueue.Enqueue(output);
        }
    }
    
    
    protected virtual void InitializeThread() { }


    protected abstract T ProcessChunk(Chunk chunk);


    private void ReleaseUnmanagedResources()
    {
        _shouldStop = true;
        _thread.Join();
    }


    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            // Release managed resources here
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    ~ChunkProcessorThread()
    {
        Dispose(false);
    }
}