using System.Diagnostics;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Threading.Pooling;

namespace BlockEngine.Client.Threading.Jobs;

/// <summary>
/// A job for the <see cref="Pooling.ThreadPool"/> with a generic result type.
/// </summary>
public abstract class VektorJob<T> : IVektorJob, IAwaitable<T>
{
    /// <summary>
    /// Context in which the job was constructed.
    /// </summary>
    protected readonly SynchronizationContext Context;

    private Action? _continuation;
    private T _result;

    /// <summary>
    /// Completion state of the job.
    /// </summary>
    public JobCompletionState CompletionState { get; protected set; }


    /// <summary>
    /// Create a new VektorJob instance.
    /// The context of the constructing thread is stored for async/await.
    /// Make sure you construct your job on the same thread that will await the result.
    /// </summary>
    protected VektorJob()
    {
        Debug.Assert(SynchronizationContext.Current != null, "SynchronizationContext.Current != null");
        Context = SynchronizationContext.Current;
        _continuation = null;
        _result = default!;
        CompletionState = JobCompletionState.None;
    }


    /// <summary>
    /// Do your main work here.
    /// Anything used within this method should be thread-safe.
    /// </summary>
    public abstract void Execute();


    /// <summary>
    /// Sets the result of the job.
    /// </summary>
    protected void SetResult(T result)
    {
        _result = result;
    }


    /// <summary>
    /// Signals that the job has completed.
    /// The thread-pool will call this automatically with a state of "Completed" unless you call it explicitly.
    /// </summary>
    public void SignalCompletion(JobCompletionState completionState)
    {
        if (completionState == JobCompletionState.None)
        {
            Logger.LogError("Signal completion called with a state of 'None'!");
            return;
        }

        if (CompletionState != JobCompletionState.None)
        {
            Logger.LogError("Signal completion called multiple times in job!");
            return;
        }

        CompletionState = completionState;
        Action? continuation = Interlocked.Exchange(ref _continuation, null);
        if (continuation != null)
            Context.Post(state => { ((Action)state!).Invoke(); }, continuation);
    }


    /// <summary>
    /// Dispatches the job to be executed by the thread-pool.
    /// </summary>
    public VektorJob<T> Dispatch()
    {
        return GlobalThreadPool.DispatchJob(this);
    }


    /// <summary>
    /// Dispatches a given action to be executed on the context which created this job.
    /// </summary>
    /// <param name="a"></param>
    protected void DispatchToContext(Action a)
    {
        Context.Post(state => { a?.Invoke(); }, null);
    }


    /// <summary>
    /// Dispatches a given action to be executed on the Unity main thread.
    /// </summary>
    protected void DispatchToMain(Action a, QueueType queueType)
    {
        GlobalThreadPool.DispatchOnMain(a, queueType);
    }


    // Custom awaiter pattern.
    public bool IsCompleted => CompletionState != JobCompletionState.None;


    public virtual T GetResult()
    {
        return _result;
    }


    public void OnCompleted(Action continuation)
    {
        Volatile.Write(ref _continuation, continuation);
    }


    public IAwaitable<T> GetAwaiter()
    {
        return this;
    }


    /// <summary>
    /// Blocks the caller until all specified jobs have completed.
    /// </summary>
    public static void WhenAll(IEnumerable<IVektorJob> jobs)
    {
        bool complete = false;
        while (!complete)
        {
            complete = true;
            foreach (IVektorJob job in jobs)
            {
                if (job.CompletionState != JobCompletionState.None) continue;
                complete = false;
                break;
            }
        }
    }
}

/// <summary>
/// Basic implementation of a VektorJob who's result is just the completion state.
/// </summary>
public abstract class VektorJob : VektorJob<JobCompletionState>
{
    public override JobCompletionState GetResult()
    {
        return CompletionState;
    }
}