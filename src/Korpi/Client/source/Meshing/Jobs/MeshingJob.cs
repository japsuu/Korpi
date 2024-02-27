using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading;
using KorpiEngine.Core.Threading.Jobs;
using KorpiEngine.Core.Threading.Pooling;

namespace Korpi.Client.Meshing.Jobs;

public class MeshingJob : KorpiJob
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(MeshingJob));
    private static readonly ObjectPool<Stopwatch> StopwatchPool = new(64, () => new Stopwatch());
    
    private readonly long _id;
    private readonly Chunk _chunk;
    private readonly Action _callback;
    private readonly float _priority;

    public override float GetPriority() => _priority;


    public MeshingJob(long id, Chunk chunk, Action callback, bool isReMeshOperation)
    {
        Debug.Assert(callback != null, nameof(callback) + " != null");
        
        _id = id;
        _chunk = chunk;
        _callback = callback;
        
        //TODO: Set the priority based on the distance to the player.
        _priority = isReMeshOperation ? WorkItemPriority.HIGHEST : WorkItemPriority.HIGH;
        
        Interlocked.Increment(ref DebugStats.ChunksWaitingMeshing);
    }


    public override void Execute()
    {
        Interlocked.Decrement(ref DebugStats.ChunksWaitingMeshing);

        // Abort the job if the chunk's job ID does not match the job ID.
        if (_chunk.CurrentJobId != _id)
        {
            Logger.Warn($"Aborting orphaned job with ID: {_id}");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        if (!GameWorld.CurrentGameWorld.ChunkManager.ChunkExistsAt(_chunk.Position))
        {
            Logger.Warn($"Aborting meshing job with ID {_id} because chunk at position {_chunk.Position} no longer exists.");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        // Acquire a read lock on the chunk and generate mesh data.
        if (_chunk.ThreadLock.TryEnterReadLock(Constants.JOB_LOCK_TIMEOUT_MS))  //WARN: This lock might not be necessary.
        {
            Stopwatch timer = StopwatchPool.Get();
            timer.Restart();

            if (ChunkMesher.IsDisposed)
            {
                _chunk.ThreadLock.ExitReadLock();
                SignalCompletion(JobCompletionState.Aborted);
                return;
            }
            
            ChunkMesh mesh = ChunkMesher.ThreadLocalInstance.GenerateMesh(_chunk);
        
            timer.Stop();
            DebugStats.PostMeshingTime(timer.ElapsedMilliseconds);
            StopwatchPool.Return(timer);
            
            _chunk.ThreadLock.ExitReadLock();

            // Signal completion.
            SignalCompletion(JobCompletionState.Completed);

            // Invoke callback on main.
            DispatchToMain(() =>
            {
                _chunk.UpdateMesh(mesh);
                _callback.Invoke();
            }, QueueType.Throttled);
        }
        else
        {
            Logger.Error($"Job with ID {_id} has encountered a deadlock and will be aborted.");

            SignalCompletion(JobCompletionState.Aborted);

            // This honestly gets us into an invalid state that cannot be recovered from, so we just quit.
            DispatchToMain(() => throw new Exception($"Job with ID {_id} aborted: Failed to acquire read lock on chunk."), QueueType.Default);
        }
    }
}