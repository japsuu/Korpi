using System.Diagnostics;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Rendering.Chunks;
using BlockEngine.Client.Threading.Jobs;
using BlockEngine.Client.Threading.Pooling;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;

namespace BlockEngine.Client.Meshing.Jobs;

public class MeshingJob : VektorJob
{
    private readonly long _id;
    private readonly Chunk _chunk;
    private readonly Action _callback;


    public MeshingJob(long id, Chunk chunk, Action callback)
    {
        Debug.Assert(callback != null, nameof(callback) + " != null");
        
        _id = id;
        _chunk = chunk;
        _callback = callback;
#if DEBUG
        Interlocked.Increment(ref Debugging.DebugStats.ChunksInMeshingQueue);
#endif
    }


    public override void Execute()
    {
#if DEBUG
        Interlocked.Decrement(ref Debugging.DebugStats.ChunksInMeshingQueue);
#endif
        // Abort the job if the chunk's job ID does not match the job ID.
        if (_chunk.CurrentJobId != _id)
        {
            Logger.LogWarning($"Aborting orphaned job with ID: {_id}");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        if (!GameWorld.CurrentGameWorld.RegionManager.ChunkExistsAt(_chunk.Position))
        {
            Logger.LogWarning($"Aborting meshing job with ID {_id} because chunk at position {_chunk.Position} no longer exists.");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        // Acquire a read lock on the chunk and generate mesh data.
        if (_chunk.ThreadLock.TryEnterReadLock(Constants.JOB_LOCK_TIMEOUT_MS))
        {
            ChunkMesh mesh = ChunkMesher.ThreadLocalInstance.GenerateMesh(_chunk);
            
            _chunk.ThreadLock.ExitReadLock();

            // Signal completion.
            SignalCompletion(JobCompletionState.Completed);

            // Invoke callback on main.
            DispatchToMain(() =>
            {
                ChunkRendererStorage.AddOrUpdateChunkMesh(mesh);
                _callback.Invoke();
            }, QueueType.Throttled);
        }
        else
        {
            Logger.LogError($"Job with ID {_id} has encountered a deadlock and will be aborted.");

            SignalCompletion(JobCompletionState.Aborted);

            // This honestly gets us into an invalid state that cannot be recovered from, so we just quit.
            DispatchToMain(() => throw new Exception($"Job with ID {_id} aborted: Failed to acquire read lock on chunk."), QueueType.Default);
        }
    }
}