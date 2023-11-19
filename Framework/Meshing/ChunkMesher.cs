using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Chunks;
using BlockEngine.Framework.Rendering.ImGuiWindows;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;


public class ChunkMesher
{
    private const int MAX_CHUNKS_MESHED_PER_FRAME = 1;

    private readonly ChunkManager _chunkManager;
    
    /// <summary>
    /// Priority queue of chunks to mesh.
    /// Chunks are prioritized by their distance to the camera.
    /// </summary>
    private readonly PriorityQueue<Vector3i, float> _chunkMeshingQueue;
    private readonly HashSet<Vector3i> _queuedChunks;

    /// <summary>
    /// Data of the chunk currently being meshed.
    /// Also includes one block wide border extending into the neighbouring chunks.
    /// </summary>
    private readonly MeshingDataCache _meshingDataCache = new(Constants.CHUNK_SIZE);


    public ChunkMesher(ChunkManager chunkManager)
    {
        _chunkManager = chunkManager;
        _chunkMeshingQueue = new PriorityQueue<Vector3i, float>();
        _queuedChunks = new HashSet<Vector3i>();
    }


    public void ProcessMeshingQueue()
    {
        RenderingWindow.RenderingStats.ChunksInMeshingQueue = _chunkMeshingQueue.Count;
        RenderingWindow.RenderingStats.StartMeshing();
        
        int chunksMeshed = 0;
        while (chunksMeshed < MAX_CHUNKS_MESHED_PER_FRAME && _chunkMeshingQueue.Count > 0)
        {
            Vector3i chunkPos = _chunkMeshingQueue.Dequeue();
            _queuedChunks.Remove(chunkPos);
            ChunkMesh mesh = GenerateMesh(chunkPos);
            ChunkMeshStorage.AddMesh(chunkPos, mesh);
            chunksMeshed++;
        }
        
        RenderingWindow.RenderingStats.StopMeshing();
    }


    public void EnqueueChunkForMeshing(Vector3i chunkOriginPos, Vector3 cameraPos)
    {
        if (_queuedChunks.Contains(chunkOriginPos))
            return;
        
        float distanceToCamera = (chunkOriginPos - cameraPos).LengthSquared;
        _chunkMeshingQueue.Enqueue(chunkOriginPos, distanceToCamera);
        _queuedChunks.Add(chunkOriginPos);
    }


    private ChunkMesh GenerateMesh(Vector3i chunkOriginPos)
    {
        if (!_chunkManager.FillMeshingArray(chunkOriginPos, _meshingDataCache))
        {
            // Not actually an issue, but leave this here for now...
            Logger.LogWarning($"Tried to mesh non-loaded chunk at {chunkOriginPos}!");
        }
        
        // Mesh the chunk based on the data cache.
        return null;
    }
    
    
    private static ushort GetBlockTextureIndex(Block block, Orientation normal) => 0;
}


/// <summary>
/// Represents the mesh of a single chunk.
/// Contains:
/// - The vertex data.
/// - The chunk position.
/// - The VAO.
/// - The VBO.
/// </summary>
public class ChunkMesh
{
    
}