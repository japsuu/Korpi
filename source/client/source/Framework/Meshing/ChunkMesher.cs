using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Threading;

namespace BlockEngine.Client.Framework.Meshing;

public class ChunkMesher : ChunkProcessorThreadManager<ChunkMesherThread, ChunkMesh>
{
    public ChunkMesher() : base(new ChunkMesherThread(), 64)
    {
    }
    
    
    public override void ProcessQueues()
    {
        RenderingStats.ChunksInMeshingQueue = (ulong)InputQueue.Count;
        RenderingStats.StartProcessMeshingQueues();
        
        base.ProcessQueues();
        
        RenderingStats.StopProcessMeshingQueues();
    }


    protected override void OnChunkProcessed(ChunkMesh output)
    {
        // Check if the chunk is still loaded
        Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(output.ChunkPos);
        if (chunk == null)
            return;
            
        ChunkRendererStorage.AddOrUpdateChunkMesh(output);
        chunk.OnMeshed();
    }
}