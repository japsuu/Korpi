using System.Collections.Concurrent;
using ConcurrentCollections;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public abstract class ChunkProcessorThread : IDisposable
{
    private volatile bool _shouldStop;
    
    private readonly Thread _thread;
    private ConcurrentHashSet<Vector3i>? _inputQueueLookup;
    private ConcurrentQueue<Vector3i>? _inputQueue;
    private ConcurrentQueue<Vector3i>? _outputQueue;


    protected ChunkProcessorThread()
    {
        _thread = new Thread(ProcessQueues);
    }


    public void StartProcessQueues(ConcurrentHashSet<Vector3i> inputQueueLookup, ConcurrentQueue<Vector3i> inputQueue, ConcurrentQueue<Vector3i> outputQueue)
    {
        _inputQueueLookup = inputQueueLookup;
        _inputQueue = inputQueue;
        _outputQueue = outputQueue;
        
        _thread.Start();
    }


    private void ProcessQueues()
    {
        if (_inputQueueLookup == null || _inputQueue == null || _outputQueue == null)
            throw new InvalidOperationException($"{nameof(ChunkProcessorThread)} was not bound to queues when started!");
        
        while (!_shouldStop)
        {
            if (!_inputQueue.TryDequeue(out Vector3i chunkPos))
                continue;
            
            Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(chunkPos);
            if (chunk == null)
                continue;
                
            ProcessChunk(chunk);
            _inputQueueLookup.TryRemove(chunkPos);
            _outputQueue.Enqueue(chunkPos);
        }
    }


    protected abstract void ProcessChunk(Chunk chunk);


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