using System.Diagnostics;

namespace BlockEngine.Client.Framework.Debugging;

public static class RenderingStats
{
    public const int AVERAGE_CHUNK_MESHING_TIME_SAMPLES = 32;
        
    public static int LoadedColumnCount;
    public static ulong ChunksInGenerationQueue;
    public static ulong ChunksInMeshingQueue;
    public static float MeshingTime { get; private set; }
    public static float AverageChunkMeshingTime { get; private set; }
        
    private static readonly Stopwatch MeshingTimer = new();
    private static readonly Stopwatch ChunkMeshingTimer = new();
    private static readonly Queue<float> ChunkMeshingTimes = new();
        
        
    public static void StartMeshing()
    {
        MeshingTimer.Restart();
    }
        
        
    public static void StopMeshing(int chunksMeshed)
    {
        MeshingTimer.Stop();
        MeshingTime = MeshingTimer.ElapsedMilliseconds / (float)chunksMeshed;
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