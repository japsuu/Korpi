using System.Diagnostics;

namespace BlockEngine.Client.Framework.Debugging;

public static class RenderingStats
{
    public static ulong LoadedColumnCount;
    public static ulong ChunksInMeshingQueue;
    public static float MeshingTime { get; private set; }
        
    private static readonly Stopwatch MeshingTimer = new();
        
        
    public static void StartMeshing()
    {
        MeshingTimer.Restart();
    }
        
        
    public static void StopMeshing(int chunksMeshed)
    {
        MeshingTimer.Stop();
        MeshingTime = MeshingTimer.ElapsedMilliseconds / (float)chunksMeshed;
    }
}