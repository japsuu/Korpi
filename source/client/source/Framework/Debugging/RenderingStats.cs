using System.Diagnostics;

namespace BlockEngine.Client.Framework.Debugging;

public static class RenderingStats
{
    private const int AVERAGE_CHUNK_GENERATION_TIME_SAMPLES = 64;
    private const int AVERAGE_CHUNK_MESHING_TIME_SAMPLES = 64;
        
    public static int LoadedColumnCount;
    public static ulong ChunksInGenerationQueue;
    public static ulong ChunksInMeshingQueue;

    public static float AverageChunkGenerationTime
    {
        get => averageChunkGenerationTime;
        private set => averageChunkGenerationTime = value;
    }

    public static float AverageChunkMeshingTime
    {
        get => averageChunkMeshingTime;
        private set => averageChunkMeshingTime = value;
    }

    private static readonly Stopwatch ChunkGenerationTimer = new();
    private static readonly Stopwatch ChunkMeshingTimer = new();
    private static readonly Queue<float> ChunkGenerationTimes = new();
    private static readonly Queue<float> ChunkMeshingTimes = new();
    private static volatile float averageChunkGenerationTime;
    private static volatile float averageChunkMeshingTime;
    
    
    public static void StartChunkGeneration()
    {
        ChunkGenerationTimer.Restart();
    }
    
    
    public static void StopChunkGeneration()
    {
        ChunkGenerationTimer.Stop();
        ChunkGenerationTimes.Enqueue(ChunkGenerationTimer.ElapsedMilliseconds);
        if (ChunkGenerationTimes.Count > AVERAGE_CHUNK_GENERATION_TIME_SAMPLES)
            ChunkGenerationTimes.Dequeue();
        
        AverageChunkGenerationTime = ChunkGenerationTimes.Sum() / ChunkGenerationTimes.Count;
    }
    
    
    public static void StartChunkMeshing()
    {
        ChunkMeshingTimer.Restart();
    }
    
    
    public static void StopChunkMeshing()
    {
        ChunkMeshingTimer.Stop();
        ChunkMeshingTimes.Enqueue(ChunkMeshingTimer.ElapsedMilliseconds);
        if (ChunkMeshingTimes.Count > AVERAGE_CHUNK_MESHING_TIME_SAMPLES)
            ChunkMeshingTimes.Dequeue();
        
        AverageChunkMeshingTime = ChunkMeshingTimes.Sum() / ChunkMeshingTimes.Count;
    }
}