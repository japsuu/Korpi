using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

public class LodChunkMesh
{
    public readonly ChunkMesh[] Meshes;
    public readonly Vector3i ChunkPos;


    public LodChunkMesh(Vector3i chunkPos)
    {
        Meshes = new ChunkMesh[Constants.TERRAIN_LOD_LEVEL_COUNT];
        ChunkPos = chunkPos;
    }
    
    
    public void SetMesh(int lodLevel, ChunkMesh mesh)
    {
        Meshes[lodLevel] = mesh;
    }
}