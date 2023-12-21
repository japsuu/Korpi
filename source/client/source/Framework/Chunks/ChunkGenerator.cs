using BlockEngine.Client.Framework.Debugging;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkGenerator : ChunkProcessorThreadManager<ChunkGeneratorThread, Vector3i>
{
    public ChunkGenerator() : base(new ChunkGeneratorThread(), 64)
    {
    }
    
    
    public override void ProcessQueues()
    {
#if DEBUG
        RenderingStats.ChunksInGenerationQueue = (ulong)InputQueue.Count;
#endif
        base.ProcessQueues();
    }


    protected override void OnChunkProcessed(Vector3i output)
    {
        Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(output);

        chunk?.OnGenerated();
    }
}