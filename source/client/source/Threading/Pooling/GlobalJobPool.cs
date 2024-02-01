using System.Collections.Concurrent;
using Korpi.Client.Logging;
using Korpi.Client.Threading.Jobs;
using Korpi.Client.Window;

namespace Korpi.Client.Threading.Pooling;

/// <summary>
/// A static pool for executing jobs.
/// Contains queues for pushing callbacks to the main thread.
/// Jobs don't necessarily have to use the queues for main-thread callbacks.
/// A sync context could be used instead.
/// </summary>
public static class GlobalJobPool
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GlobalJobPool));

    /// <summary>
    /// Maximum number of <see cref="mainQueueThrottled"/> invocations executed per tick.
    /// </summary>
    private const int MAX_THROTTLED_UPDATES_PER_TICK = 64;

    /// <summary>
    /// The job pool.
    /// </summary>
    private static IJobPool jobPool = null!;

    /// <summary>
    /// A queue of actions to be executed on the main thread.
    /// </summary>
    private static ConcurrentQueue<Action> mainQueue = null!;

    /// <summary>
    /// A throttled queue of actions to be executed on the main thread.
    /// </summary>
    private static ConcurrentQueue<Action> mainQueueThrottled = null!;

    /// <summary>
    /// The number of threads allocated to process the pool.
    /// </summary>
    public static uint ThreadCount { get; private set; }


    public static void Initialize()
    {
        // Since we're CPU-bound (most of the threads will be waiting in a loop), allocate only 3/4 of the system's logical processor count with a minimum of 2.
        //NOTE: For some reason, the Debug build seems to run faster than Release. https://stackoverflow.com/questions/8858128/c-opengl-application-running-smoother-with-debugger-attached
#if DEBUG
        ThreadCount = (uint)Math.Max(SystemInfo.ProcessorCount * 3 / 4, 2);
        Logger.Warn($"[Global Thread Pool] Running in DEBUG, using {ThreadCount} threads instead of the usual {SystemInfo.ProcessorCount/4}.");
#else
        ThreadCount = (uint)Math.Max(SystemInfo.ProcessorCount / 4, 2);
#endif
        jobPool = new JobTplPool();
        // jobPool = new JobSingleThreadPool();
        // jobPool = new JobThreadPool(ThreadCount, ThreadConfig.Default());
        mainQueue = new ConcurrentQueue<Action>();
        mainQueueThrottled = new ConcurrentQueue<Action>();
        Logger.Info($"Initialized with {ThreadCount} threads.");
    }
    
    
    public static void Shutdown()
    {
        jobPool.Shutdown();
        Logger.Info("Shutdown.");
    }


    public static void Update()
    {
        Debugging.DebugStats.ItemsInMainThreadQueue = (ulong)mainQueue.Count;
        // Process the regular queue.
        while (mainQueue.TryDequeue(out Action? a))
            a.Invoke();
    }


    public static void FixedUpdate()
    {
        Debugging.DebugStats.ItemsInMainThreadThrottledQueue = (ulong)mainQueueThrottled.Count;
        Debugging.DebugStats.MainThreadThrottledQueueItemsPerTick = MAX_THROTTLED_UPDATES_PER_TICK;

        // Process the throttled queue.
        int count = MAX_THROTTLED_UPDATES_PER_TICK;
        while (mainQueueThrottled.TryDequeue(out Action? a))
        {
            a.Invoke();
            count--;

            if (count <= 0)
                break;
        }
        
        jobPool.FixedUpdate();
    }


    /// <summary>
    /// Immediately queues the provided job for execution on the pool.
    /// </summary>
    public static KorpiJob<T> DispatchJob<T>(KorpiJob<T> item)
    {
        jobPool.EnqueueWorkItem(item);
        return item;
    }


    /// <summary>
    /// Queues a given action to be executed on the main thread.
    /// </summary>
    public static void DispatchOnMain(Action a, QueueType queue)
    {
        switch (queue)
        {
            case QueueType.Default:
                mainQueue.Enqueue(a);
                break;
            case QueueType.Throttled:
                mainQueueThrottled.Enqueue(a);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(queue), queue, null);
        }
    }
}