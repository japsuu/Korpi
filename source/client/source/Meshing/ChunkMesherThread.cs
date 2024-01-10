using Korpi.Client.Threading;
using Korpi.Client.World.Regions.Chunks;

namespace Korpi.Client.Meshing;

public class ChunkMesherThread : ChunkProcessorThread<ChunkMesh>
{
    protected override ChunkMesh ProcessChunk(Chunk chunk)
    {
        ChunkMesh mesh = ChunkMesher.ThreadLocalInstance.GenerateMesh(chunk);

        return mesh;
    }
}