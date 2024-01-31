using Korpi.Client.Configuration;
using Korpi.Client.Logging;
using Korpi.Client.Threading.Jobs;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;

namespace Korpi.Client.Generation.Jobs;

public class GenerationJob : KorpiJob
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GenerationJob));

    private readonly long _id;
    private readonly ChunkColumn _chunkColumn;
    private readonly Action _callback;


    public GenerationJob(long id, ChunkColumn chunkColumn, Action callback)
    {
        _id = id;
        _chunkColumn = chunkColumn;
        _callback = callback;
        
        Interlocked.Increment(ref Debugging.DebugStats.ChunksInGenerationQueue);
    }


    public override void Execute()
    {
        Interlocked.Decrement(ref Debugging.DebugStats.ChunksInGenerationQueue);
        
        // Abort the job if the chunkColumn's job ID does not match the job ID.
        if (_chunkColumn.CurrentJobId != _id)
        {
            Logger.Warn($"Aborting orphaned job with ID: {_id}");
            SignalCompletion(JobCompletionState.Aborted);
            return;
        }

        // Acquire a read lock on the chunkColumn and generate terrain data.
        if (_chunkColumn.ThreadLock.TryEnterWriteLock(Constants.JOB_LOCK_TIMEOUT_MS))  //WARN: This lock might not be necessary.
        {
            GameWorld.CurrentGameWorld.TerrainGenerator.ProcessChunk(_chunkColumn);
            
            _chunkColumn.ThreadLock.ExitWriteLock();

            // Signal completion.
            SignalCompletion(JobCompletionState.Completed);

            // Invoke callback on main.
            DispatchToMain(_callback, QueueType.Throttled);
        }
        else
        {
            Logger.Error($"Job with ID {_id} has encountered a deadlock and will be aborted.");

            SignalCompletion(JobCompletionState.Aborted);

            // This honestly gets us into an invalid state that cannot be recovered from, so we just quit.
            DispatchToMain(() => throw new Exception($"Job with ID {_id} aborted: Failed to acquire read lock on chunkColumn."), QueueType.Default);
        }
    }
}