using BlockEngine.Client.Framework.Debugging;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkGenerator : ChunkProcessorThreadManager<ChunkGeneratorThread>
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


    protected override void OnChunkProcessed(Chunk chunk)
    {
        chunk.OnGenerated();
    }
}