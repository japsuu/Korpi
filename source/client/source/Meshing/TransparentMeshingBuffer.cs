using System.Diagnostics.CodeAnalysis;

namespace Korpi.Client.Meshing;

public class TransparentMeshingBuffer : MeshingBuffer
{
    // Since different adjacent transparent blocks ignore the face culling and are both rendered, the worst case is all faces are rendered.
    private const int MAX_VISIBLE_FACES = FACES_PER_BLOCK * CHUNK_SIZE_CUBED;
    private const int MAX_VERTICES_PER_CHUNK = MAX_VISIBLE_FACES * VERTICES_PER_FACE;
    private const int MAX_VERTEX_DATA_PER_CHUNK = MAX_VERTICES_PER_CHUNK * ELEMENTS_PER_VERTEX;
    private const int MAX_INDICES_PER_CHUNK = MAX_VISIBLE_FACES * INDICES_PER_FACE;
    
    // Suppress because this is a constant one-time memory allocation.
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.UInt32[]")]
    public TransparentMeshingBuffer() : base(MAX_VERTEX_DATA_PER_CHUNK, MAX_INDICES_PER_CHUNK)
    {
    }
}