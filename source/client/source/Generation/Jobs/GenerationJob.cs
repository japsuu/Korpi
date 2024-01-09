using BlockEngine.Client.Logging;
using BlockEngine.Client.Threading.Jobs;
using BlockEngine.Client.Threading.Pooling;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;

namespace BlockEngine.Client.Generation.Jobs;

public class GenerationJob : KorpiJob
{
    private readonly long _id;
    private readonly Chunk _chunk;
    private readonly Action _callback;


    public GenerationJob(long id, Chunk chunk, Action callback)
    {
        _id = id;
        _chunk = chunk;
        _callback = callback;
#if DEBUG
        Interlocked.Increment(ref Debugging.DebugStats.ChunksInGenerationQueue);
#endif
    }


    public override void Execute()
    {
#if DEBUG
        Interlocked.Decrement(ref Debugging.DebugStats.ChunksInGenerationQueue);
#endif
        // Abort the job if the chunk's job ID does not match the job ID.
        if (_chunk.CurrentJobId != _id)
        {
            Logger.LogWarning($"Aborting orphaned job with ID: {_id}");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        // Acquire a read lock on the chunk and generate terrain data.
        if (_chunk.ThreadLock.TryEnterWriteLock(Constants.JOB_LOCK_TIMEOUT_MS))
        {
            GameWorld.CurrentGameWorld.TerrainGenerator.ProcessChunk(_chunk);
            
            _chunk.ThreadLock.ExitWriteLock();

            // Signal completion.
            SignalCompletion(JobCompletionState.Completed);

            // Invoke callback on main.
            DispatchToMain(_callback, QueueType.Throttled);
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