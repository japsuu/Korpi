using System.Diagnostics;

namespace BlockEngine.Client.Debugging;

public static class DebugStats
{
    private const int AVERAGE_CHUNK_GENERATION_TIME_SAMPLES = 64;
    private const int AVERAGE_CHUNK_MESHING_TIME_SAMPLES = 64;
        
    public static int LoadedRegionCount;
    public static ulong ChunksInGenerationQueue;
    public static ulong ChunksInMeshingQueue;

    public static float AverageChunkGenerationTime;
    public static float AverageChunkMeshingTime;
    public static float MedianChunkGenerationTime;
    public static float MedianChunkMeshingTime;

    public static float MinChunkGenerationTime = float.MaxValue;
    public static float MaxChunkGenerationTime = float.MinValue;
    public static float MinChunkMeshingTime = float.MaxValue;
    public static float MaxChunkMeshingTime = float.MinValue;

    private static readonly Stopwatch ChunkGenerationTimer = new();
    private static readonly Stopwatch ChunkMeshingTimer = new();
    private static readonly Queue<float> ChunkGenerationTimes = new();
    private static readonly Queue<float> ChunkMeshingTimes = new();
    private static readonly object LockObject = new();
    
    
    public static void StartChunkGeneration()
    {
        lock (LockObject)
        {
            ChunkGenerationTimer.Restart();
        }
    }
    
    
    public static void StopChunkGeneration()
    {
        lock (LockObject)
        {
            ChunkGenerationTimer.Stop();
            ChunkGenerationTimes.Enqueue(ChunkGenerationTimer.ElapsedMilliseconds);
            if (ChunkGenerationTimes.Count > AVERAGE_CHUNK_GENERATION_TIME_SAMPLES)
                ChunkGenerationTimes.Dequeue();

            float sum = 0;
            foreach (float time in ChunkGenerationTimes)
            {
                sum += time;
                
                if (time < MinChunkGenerationTime)
                    MinChunkGenerationTime = time;
                
                if (time > MaxChunkGenerationTime)
                    MaxChunkGenerationTime = time;
            }
            AverageChunkGenerationTime = sum / ChunkGenerationTimes.Count;
            
            // Calculate the median
            float[] sortedTimes = ChunkGenerationTimes.ToArray();
            Array.Sort(sortedTimes);
            MedianChunkGenerationTime = sortedTimes[sortedTimes.Length / 2];
        }
    }
    
    
    public static void StartChunkMeshing()
    {
        lock (LockObject)
        {
            ChunkMeshingTimer.Restart();
        }
    }
    
    
    public static void StopChunkMeshing()
    {
        lock (LockObject)
        {
            ChunkMeshingTimer.Stop();
            ChunkMeshingTimes.Enqueue(ChunkMeshingTimer.ElapsedMilliseconds);
            if (ChunkMeshingTimes.Count > AVERAGE_CHUNK_MESHING_TIME_SAMPLES)
                ChunkMeshingTimes.Dequeue();

            float sum = 0;
            foreach (float time in ChunkMeshingTimes)
            {
                sum += time;
                
                if (time < MinChunkMeshingTime)
                    MinChunkMeshingTime = time;
                
                if (time > MaxChunkMeshingTime)
                    MaxChunkMeshingTime = time;
            }
            AverageChunkMeshingTime = sum / ChunkMeshingTimes.Count;
            
            // Calculate the median
            float[] sortedTimes = ChunkMeshingTimes.ToArray();
            Array.Sort(sortedTimes);
            MedianChunkMeshingTime = sortedTimes[sortedTimes.Length / 2];
        }
    }
}