using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Korpi.Client.Logging;
using Korpi.Client.Threading.Jobs;

namespace Korpi.Client.Threading.Pooling;

public sealed class JobTplPool : IJobPool
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(JobSingleThreadPool));
    
    private readonly ConcurrentQueue<IKorpiJob> _workQueue = new();
    private readonly ActionBlock<IKorpiJob> _jobProcessor = new(
        ExecuteJob,
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            BoundedCapacity = -1,
        });


    public JobTplPool()
    {
    }


    private static void ExecuteJob(IKorpiJob job)
    {
        try
        {
            job.Execute();

            if (job.CompletionState == JobCompletionState.None)
                job.SignalCompletion(JobCompletionState.Completed);
        }
        catch (Exception e)
        {
            Logger.Error("Worker thread encountered an exception while executing a job:", e);

            if (job.CompletionState == JobCompletionState.None)
                job.SignalCompletion(JobCompletionState.Aborted);
        }
    }


    public void EnqueueWorkItem(IKorpiJob korpiJob, WorkItemPriority priority)
    {
        _workQueue.Enqueue(korpiJob);
    }


    public void FixedUpdate()
    {
        while (_workQueue.TryDequeue(out IKorpiJob? job))
        {
            _jobProcessor.Post(job);
        }
    }


    public void Shutdown()
    {
        
    }
}