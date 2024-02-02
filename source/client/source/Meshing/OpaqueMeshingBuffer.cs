namespace Korpi.Client.Meshing;

public class OpaqueMeshingBuffer : MeshingBuffer
{
    // Since we cull internal faces, the worst case would be half of the faces (every other block needs to be meshed).
    private const int MAX_VISIBLE_FACES = FACES_PER_BLOCK * CHUNK_SIZE_CUBED / 2;
    private const int MAX_VERTICES_PER_CHUNK = MAX_VISIBLE_FACES * VERTICES_PER_FACE;
    private const int MAX_VERTEX_DATA_PER_CHUNK = MAX_VERTICES_PER_CHUNK * ELEMENTS_PER_VERTEX;
    private const int MAX_INDICES_PER_CHUNK = MAX_VISIBLE_FACES * INDICES_PER_FACE;
    
    public OpaqueMeshingBuffer() : base(MAX_VERTEX_DATA_PER_CHUNK, MAX_INDICES_PER_CHUNK)
    {
    }
}