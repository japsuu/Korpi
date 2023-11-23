using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Chunks;
using BlockEngine.Framework.Rendering.ImGuiWindows;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;


public class ChunkMesher
{
    private const int MAX_CHUNKS_MESHED_PER_FRAME = 2;

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
    private readonly MeshingBuffer _meshingBuffer = new();
    
    private static readonly Vector3i[] NeighbourOffsets =
    {
        new(1, 0, 0),
        new(0, 1, 0),
        new(0, 0, 1),
        new(-1, 0, 0),
        new(0, -1, 0),
        new(0, 0, -1),
    };


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
            ChunkMesh? mesh = GenerateMesh(chunkPos);
            if (mesh == null)
                continue;
            ChunkMeshStorage.AddMesh(chunkPos, mesh);
            chunksMeshed++;
        }
        
        RenderingWindow.RenderingStats.StopMeshing(chunksMeshed);
    }


    public void EnqueueChunkForInitialMeshing(Vector3i chunkOriginPos, Vector3 cameraPos)
    {
        if (_queuedChunks.Contains(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already queued!");

        if (ChunkMeshStorage.ContainsMesh(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already meshed!");

        float distanceToCamera = (chunkOriginPos - cameraPos).LengthSquared;
        _chunkMeshingQueue.Enqueue(chunkOriginPos, distanceToCamera);
        _queuedChunks.Add(chunkOriginPos);
    }


    public void EnqueueChunkForMeshing(Vector3i chunkOriginPos, Vector3 cameraPos)
    {
        if (_queuedChunks.Contains(chunkOriginPos))
            return;

        if (ChunkMeshStorage.ContainsMesh(chunkOriginPos))
        {
            Logger.LogWarning($"Trashing the mesh of chunk at {chunkOriginPos}!");
            DeleteChunkMesh(chunkOriginPos);
        }
        
        float distanceToCamera = (chunkOriginPos - cameraPos).LengthSquared;
        _chunkMeshingQueue.Enqueue(chunkOriginPos, distanceToCamera);
        _queuedChunks.Add(chunkOriginPos);
    }
    
    
    public void DeleteChunkMesh(Vector3i chunkOriginPos)
    {
        ChunkMeshStorage.RemoveMesh(chunkOriginPos);
    }


    private ChunkMesh? GenerateMesh(Vector3i chunkOriginPos)
    {
        if (!_chunkManager.FillMeshingCache(chunkOriginPos, _meshingDataCache, out Chunk? chunk))
        {
            // Not actually an issue, but leave this here for now...
            Logger.LogWarning($"Tried to mesh non-loaded chunk at {chunkOriginPos}!");
            return null;
        }
        
        _meshingBuffer.Clear();
        
        // Mesh the chunk based on the data cache, using _meshingBuffer.AddFace(blockPos, faceNormal, textureIndex, lighting) to add faces to the buffer.
        // Exploit the meshing cache's spatial locality by iterating over the blocks in the order they are stored in the cache.
        for (int z = 1; z <= Constants.CHUNK_SIZE; z++)     // Start at 1 on all axis to skip the border.
        {
            for (int y = 1; y <= Constants.CHUNK_SIZE; y++)
            {
                for (int x = 1; x <= Constants.CHUNK_SIZE; x++)
                {
                    BlockState blockState = _meshingDataCache.GetData(x, y, z);
                    
                    // If the block is invisible, skip it
                    if (blockState.Visibility == BlockVisibility.Empty)
                        continue;
                    
                    // Iterate over all 6 faces of the block
                    for (int face = 0; face < 6; face++)
                    {
                        // If the face is not visible, skip it
                        // if (!blockState.Block.IsFaceVisible(blockState, (BlockFaceNormal)face))
                        //     continue;
                        
                        // If the face is not opaque, skip it
                        // if (!blockState.Block.IsFaceOpaque(blockState, (BlockFaceNormal)face))
                        //     continue;

                        Vector3i neighbourOffset = NeighbourOffsets[face];
                        int neighbourX = x + neighbourOffset.X;
                        int neighbourY = y + neighbourOffset.Y;
                        int neighbourZ = z + neighbourOffset.Z;
                        BlockState neighbour = _meshingDataCache.GetData(neighbourX, neighbourY, neighbourZ);

                        // If the neighbour is opaque, skip this face.
                        // If the neighbour is empty or transparent, we need to mesh this face.
                        if (neighbour.Visibility == BlockVisibility.Opaque)
                            continue;

                        // Get the texture index of the block face
                        ushort textureIndex = GetBlockTextureIndex(blockState, (BlockFaceNormal)face);
                        
                        // Get the lighting of the block face
                        const int lightLevel = Constants.MAX_LIGHT_LEVEL;
                        const int skyLightLevel = Constants.MAX_LIGHT_LEVEL;
                        Color9 lightColor = Color9.White;
                        
                        // Add the face to the meshing buffer
                        Vector3i blockPos = new(x - 1, y - 1, z - 1);
                        _meshingBuffer.AddFace(blockPos, (BlockFaceNormal)face, textureIndex, lightColor, lightLevel, skyLightLevel);
                    }
                }
            }
        }
        
        chunk!.IsMeshDirty = false;
        chunk.IsMeshed = true;

        return _meshingBuffer.CreateMesh(chunkOriginPos);
    }
    
    
    private static ushort GetBlockTextureIndex(BlockState block, BlockFaceNormal normal)
    {
        return 0;
    }
}