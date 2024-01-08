using System.Diagnostics;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Registries;
using BlockEngine.Client.Threading;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;
using BlockEngine.Client.World.Regions.Chunks.Blocks;
using JetBrains.Profiler.Api;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Meshing;

public class ChunkMesherThread : ChunkProcessorThread<ChunkMesh>
{
    protected override ChunkMesh ProcessChunk(Chunk chunk)
    {
        ChunkMesh mesh = ChunkMesher.ThreadLocalInstance.GenerateMesh(chunk);

        return mesh;
    }
}