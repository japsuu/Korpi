using Korpi.Client.Threading.Jobs;

namespace Korpi.Client.Threading.Pooling;

/// <summary>
/// Represents an object that can pool jobs.
/// </summary>
public interface IJobPool
{
    public void EnqueueWorkItem(IKorpiJob korpiJob, WorkItemPriority priority);
    
    
    public void FixedUpdate();
    
    
    public void Shutdown();
}