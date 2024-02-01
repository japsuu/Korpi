using System.Collections.Concurrent;
using Korpi.Client.Blocks;

namespace Korpi.Client.Debugging;

public static class DebugStats
{
    private const int AVERAGE_CHUNK_GENERATION_TIME_SAMPLES = 64;
    private const int AVERAGE_CHUNK_MESHING_TIME_SAMPLES = 64;
        
    public static int LoadedChunkCount;
    public static ulong ChunksInGenerationQueue;
    public static ulong ChunksInMeshingQueue;
    public static ulong ItemsInMainThreadQueue;
    public static ulong ItemsInMainThreadThrottledQueue;
    public static ulong MainThreadThrottledQueueItemsPerTick;

    public static float AverageChunkGenerationTime;
    public static float AverageChunkMeshingTime;
    public static float MedianChunkGenerationTime;
    public static float MedianChunkMeshingTime;

    public static float MinChunkGenerationTime = float.MaxValue;
    public static float MaxChunkGenerationTime = float.MinValue;
    public static float MinChunkMeshingTime = float.MaxValue;
    public static float MaxChunkMeshingTime = float.MinValue;
    
    public static BlockState LastRaycastResult;
    public static ulong RenderedTris;

    private static readonly ConcurrentQueue<float> ChunkGenerationTimes = new();
    private static readonly ConcurrentQueue<float> ChunkMeshingTimes = new();


    public static void CalculateStats()
    {
        CalculateGenerationTimes();
        CalculateMeshingTimes();
    }


    private static void CalculateGenerationTimes()
    {
        if (ChunkGenerationTimes.IsEmpty)
            return;
        float sum = 0;
        float[] floats = ChunkGenerationTimes.ToArray();
        foreach (float time in floats)
        {
            sum += time;
                
            if (time < MinChunkGenerationTime)
                MinChunkGenerationTime = time;
                
            if (time > MaxChunkGenerationTime)
                MaxChunkGenerationTime = time;
        }
        AverageChunkGenerationTime = sum / ChunkGenerationTimes.Count;
            
        // Calculate the median
        Array.Sort(floats);
        MedianChunkGenerationTime = floats[floats.Length / 2];
    }


    private static void CalculateMeshingTimes()
    {
        if (ChunkMeshingTimes.IsEmpty)
            return;
        float sum = 0;
        float[] floats = ChunkMeshingTimes.ToArray();
        foreach (float time in floats)
        {
            sum += time;
                
            if (time < MinChunkMeshingTime)
                MinChunkMeshingTime = time;
                
            if (time > MaxChunkMeshingTime)
                MaxChunkMeshingTime = time;
        }
        AverageChunkMeshingTime = sum / ChunkMeshingTimes.Count;
            
        // Calculate the median
        Array.Sort(floats);
        MedianChunkMeshingTime = floats[floats.Length / 2];
    }


    public static void PostMeshingTime(float time)
    {
        ChunkMeshingTimes.Enqueue(time);
        if (ChunkMeshingTimes.Count > AVERAGE_CHUNK_MESHING_TIME_SAMPLES)
            ChunkMeshingTimes.TryDequeue(out float _);
    }
    
    
    public static void PostGenerationTime(float time)
    {
        ChunkGenerationTimes.Enqueue(time);
        if (ChunkGenerationTimes.Count > AVERAGE_CHUNK_GENERATION_TIME_SAMPLES)
            ChunkGenerationTimes.TryDequeue(out float _);
    }
}