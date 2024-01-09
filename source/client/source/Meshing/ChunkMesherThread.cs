using System.Diagnostics;
using Korpi.Client.Debugging;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Regions.Chunks.Blocks;
using JetBrains.Profiler.Api;
using Korpi.Client.Threading;
using Korpi.Client.World.Regions.Chunks;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

public class ChunkMesherThread : ChunkProcessorThread<ChunkMesh>
{
    protected override ChunkMesh ProcessChunk(Chunk chunk)
    {
        ChunkMesh mesh = ChunkMesher.ThreadLocalInstance.GenerateMesh(chunk);

        return mesh;
    }
}