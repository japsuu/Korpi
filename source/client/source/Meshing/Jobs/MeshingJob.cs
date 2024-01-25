using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.Logging;
using Korpi.Client.Rendering.Chunks;
using Korpi.Client.Threading.Jobs;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;

namespace Korpi.Client.Meshing.Jobs;

public class MeshingJob : KorpiJob
{
    private readonly long _id;
    private readonly SubChunk _subChunk;
    private readonly Action _callback;


    public MeshingJob(long id, SubChunk subChunk, Action callback)
    {
        Debug.Assert(callback != null, nameof(callback) + " != null");
        
        _id = id;
        _subChunk = subChunk;
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
        // Abort the job if the subChunk's job ID does not match the job ID.
        if (_subChunk.CurrentJobId != _id)
        {
            Logger.LogWarning($"Aborting orphaned job with ID: {_id}");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        if (!GameWorld.CurrentGameWorld.ChunkManager.ChunkExistsAt(_subChunk.Position))
        {
            Logger.LogWarning($"Aborting meshing job with ID {_id} because subChunk at position {_subChunk.Position} no longer exists.");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        // Acquire a read lock on the subChunk and generate mesh data.
        if (_subChunk.ThreadLock.TryEnterReadLock(Constants.JOB_LOCK_TIMEOUT_MS))
        {
            ChunkMesh mesh = ChunkMesher.ThreadLocalInstance.GenerateMesh(_subChunk);
            
            _subChunk.ThreadLock.ExitReadLock();

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
            DispatchToMain(() => throw new Exception($"Job with ID {_id} aborted: Failed to acquire read lock on subChunk."), QueueType.Default);
        }
    }
}