using System.Diagnostics;

namespace BlockEngine.Client.Framework.Debugging;

public static class RenderingStats
{
    public const int AVERAGE_CHUNK_MESHING_TIME_SAMPLES = 32;
        
    public static int LoadedColumnCount;
    public static ulong ChunksInGenerationQueue;
    public static ulong ChunksInMeshingQueue;
    public static float MeshingQueueProcessingTime;

    public static float AverageChunkMeshingTime
    {
        get => averageChunkMeshingTime;
        private set => averageChunkMeshingTime = value;
    }

    private static readonly Stopwatch MeshingQueueTimer = new();
    private static readonly Stopwatch ChunkMeshingTimer = new();
    private static readonly Queue<float> ChunkMeshingTimes = new();
    private static volatile float averageChunkMeshingTime;


    public static void StartProcessMeshingQueues()
    {
        MeshingQueueTimer.Restart();
    }
        
        
    public static void StopProcessMeshingQueues()
    {
        MeshingQueueTimer.Stop();
        MeshingQueueProcessingTime = MeshingQueueTimer.ElapsedMilliseconds;
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