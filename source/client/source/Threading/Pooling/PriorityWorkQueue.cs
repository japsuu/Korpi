using System.Collections.Concurrent;

namespace Korpi.Client.Threading.Pooling;

/// <summary>
/// A thread-safe work queue with multiple priority levels.
/// </summary>
/// <typeparam name="T">The type of the items in the queue.</typeparam>
public class PriorityWorkQueue<T>
{
    private readonly BlockingCollection<T> _workQueueLow;
    private readonly BlockingCollection<T> _workQueueNormal;
    private readonly BlockingCollection<T> _workQueueHigh;
    private readonly BlockingCollection<T> _workQueueCritical;
    private readonly BlockingCollection<T>[] _workQueues;


    public PriorityWorkQueue()
    {
        _workQueueLow = new BlockingCollection<T>(new ConcurrentQueue<T>());
        _workQueueNormal = new BlockingCollection<T>(new ConcurrentQueue<T>());
        _workQueueHigh = new BlockingCollection<T>(new ConcurrentQueue<T>());
        _workQueueCritical = new BlockingCollection<T>(new ConcurrentQueue<T>());
        _workQueues = new[]
        {
            _workQueueCritical,
            _workQueueHigh,
            _workQueueNormal,
            _workQueueLow
        };
    }


    public bool IsCompleted => _workQueueLow.IsCompleted &&
                               _workQueueNormal.IsCompleted &&
                               _workQueueHigh.IsCompleted &&
                               _workQueueCritical.IsCompleted;

    public bool IsAddingCompleted => _workQueueLow.IsAddingCompleted &&
                                     _workQueueNormal.IsAddingCompleted &&
                                     _workQueueHigh.IsAddingCompleted &&
                                     _workQueueCritical.IsAddingCompleted;


    public void Add(T item, WorkItemPriority priority)
    {
        switch (priority)
        {
            case WorkItemPriority.Low:
                _workQueueLow.Add(item);
                break;
            case WorkItemPriority.Normal:
                _workQueueNormal.Add(item);
                break;
            case WorkItemPriority.High:
                _workQueueHigh.Add(item);
                break;
            case WorkItemPriority.Critical:
                _workQueueCritical.Add(item);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
        }
    }


    public bool TryTakeFromAny(out T? item)
    {
        int index = BlockingCollection<T>.TryTakeFromAny(_workQueues, out item);
        return index != -1;
    }


    public void CompleteAdding()
    {
        _workQueueLow.CompleteAdding();
        _workQueueNormal.CompleteAdding();
        _workQueueHigh.CompleteAdding();
        _workQueueCritical.CompleteAdding();
    }
}