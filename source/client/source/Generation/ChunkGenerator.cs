﻿using BlockEngine.Client.Debugging;
using BlockEngine.Client.Threading;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Generation;

public class ChunkGenerator : ChunkProcessorThreadManager<ChunkGeneratorThread, Vector3i>
{
    public ChunkGenerator() : base(new ChunkGeneratorThread(), 64)
    {
    }
    
    
    public override void ProcessQueues()
    {
#if DEBUG
        DebugStats.ChunksInGenerationQueue = (ulong)InputQueue.Count;
#endif
        base.ProcessQueues();
    }


    protected override void OnChunkProcessed(Vector3i output)
    {
        Chunk? chunk = GameWorld.CurrentGameWorld.RegionManager.GetChunkAt(output);

        chunk?.OnGenerated();
    }
}