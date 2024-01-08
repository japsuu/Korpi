using System.Collections.Concurrent;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Threading.Jobs;
using BlockEngine.Client.Threading.Threads;
using BlockEngine.Client.Window;

namespace BlockEngine.Client.Threading.Pooling;

/// <summary>
/// A static thread-pool.
/// Contains queues for pushing callbacks to the main thread.
/// Jobs don't necessarily have to use the queues for main-thread callbacks.
/// A sync context could be used instead.
/// </summary>
public static class GlobalThreadPool
{
    /// <summary>
    /// Minimum number of <see cref="mainQueueThrottled"/> invocations executed per tick.
    /// </summary>
    private const int MIN_THROTTLED_UPDATES_PER_TICK = 1;
    
    /// <summary>
    /// Maximum number of <see cref="mainQueueThrottled"/> invocations executed per tick.
    /// </summary>
    private const int MAX_THROTTLED_UPDATES_PER_TICK = 64;

    /// <summary>
    /// The number of invocations executed on the main thread from the throttled queue per tick.
    /// Dynamically adjusted based on performance.
    /// </summary>
    private static int throttledUpdatesPerTick;
    
    /// <summary>
    /// The thread pool.
    /// </summary>
    private static ThreadPool threadPool = null!;
    
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
        // Allocate 3/4 of the system's thread count with a minimum of 2.
        ThreadCount = (uint)Math.Max(SystemInfo.ProcessorCount * 3 / 4, 2);
        threadPool = new ThreadPool(ThreadCount, ThreadConfig.Default());
        mainQueue = new ConcurrentQueue<Action>();
        mainQueueThrottled = new ConcurrentQueue<Action>();
            
        Logger.Log($"[Global Thread Pool] Initialized with {ThreadCount} threads.");
    }


    public static void Update()
    {
        // Process the regular queue.
        while (mainQueue.TryDequeue(out Action? a))
            a.Invoke();
    }


    public static void FixedUpdate()
    {
        throttledUpdatesPerTick = DynamicPerformance.GetDynamic(MIN_THROTTLED_UPDATES_PER_TICK, MAX_THROTTLED_UPDATES_PER_TICK);

        // Process the throttled queue.
        int count = throttledUpdatesPerTick;
        while (mainQueueThrottled.TryDequeue(out Action? a))
        {
            a.Invoke();
            count--;

            if (count <= 0)
                break;
        }
    }
    
    
    /// <summary>
    /// Immediately queues the provided job for execution on the pool.
    /// </summary>
    public static VektorJob<T> DispatchJob<T>(VektorJob<T> item) {
        threadPool.EnqueueWorkItem(item);
        return item;
    }
        
    /// <summary>
    /// Queues a given action to be executed on the main thread.
    /// </summary>
    public static void DispatchOnMain(Action a, QueueType queue) {
        switch (queue) {
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