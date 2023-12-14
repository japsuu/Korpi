using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;


public class ChunkMesher
{
    private const int MAX_CHUNKS_MESHED_PER_FRAME = 8;

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
    private readonly BlockState[] _blockStateNeighbourhood = new BlockState[27];   // 27 = 3x3x3
    
    private static readonly Vector3i[] NeighbourOffsets =
    {
        new(2, 1, 1),
        new(1, 2, 1),
        new(1, 1, 2),
        new(0, 1, 1),
        new(1, 0, 1),
        new(1, 1, 0),
    };


    public ChunkMesher(ChunkManager chunkManager)
    {
        _chunkManager = chunkManager;
        _chunkMeshingQueue = new PriorityQueue<Vector3i, float>();
        _queuedChunks = new HashSet<Vector3i>();
    }


    public void ProcessMeshingQueue()
    {
        RenderingStats.ChunksInMeshingQueue = (ulong)_chunkMeshingQueue.Count;
        RenderingStats.StartMeshing();
        
        int chunksMeshed = 0;
        while (chunksMeshed < MAX_CHUNKS_MESHED_PER_FRAME && _chunkMeshingQueue.Count > 0)
        {
            Vector3i chunkPos = _chunkMeshingQueue.Dequeue();
            _queuedChunks.Remove(chunkPos);
            if (!_chunkManager.IsChunkLoaded(chunkPos))
                continue;
            RenderingStats.StartChunkMeshing();
            ChunkRenderer mesh = GenerateMesh(chunkPos);
            ChunkRendererStorage.AddRenderer(chunkPos, mesh);
            chunksMeshed++;
            RenderingStats.StopChunkMeshing();
        }
        
        RenderingStats.StopMeshing(chunksMeshed);
    }


    public void EnqueueChunkForInitialMeshing(Vector3i chunkOriginPos, Vector3 cameraPos)
    {
        if (_queuedChunks.Contains(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already queued!");

        if (ChunkRendererStorage.ContainsRenderer(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already meshed!");

        float distanceToCamera = (chunkOriginPos - cameraPos).LengthSquared;
        _chunkMeshingQueue.Enqueue(chunkOriginPos, distanceToCamera);
        _queuedChunks.Add(chunkOriginPos);
    }


    public void EnqueueChunkForMeshing(Vector3i chunkOriginPos, Vector3 cameraPos)
    {
        if (_queuedChunks.Contains(chunkOriginPos))
            return;

        if (ChunkRendererStorage.ContainsRenderer(chunkOriginPos))
        {
            Logger.LogWarning($"Trashing the mesh of chunk at {chunkOriginPos}!");
            ChunkRendererStorage.RemoveRenderer(chunkOriginPos);
        }
        
        float distanceToCamera = (chunkOriginPos - cameraPos).LengthSquared;
        _chunkMeshingQueue.Enqueue(chunkOriginPos, distanceToCamera);
        _queuedChunks.Add(chunkOriginPos);
    }


    private ChunkRenderer GenerateMesh(Vector3i chunkOriginPos)
    {
        Chunk chunk = _chunkManager.FillMeshingCache(chunkOriginPos, _meshingDataCache);
        
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
                    if (blockState.RenderType == BlockRenderType.None)
                        continue;

                    // Gather neighbourhood data
                    for (int neighbourZ = 0; neighbourZ < 3; neighbourZ++)
                    {
                        for (int neighbourY = 0; neighbourY < 3; neighbourY++)
                        {
                            for (int neighbourX = 0; neighbourX < 3; neighbourX++)
                            {
                                BlockState neighbour = _meshingDataCache.GetData(x + neighbourX - 1, y + neighbourY - 1, z + neighbourZ - 1);
                                _blockStateNeighbourhood[neighbourX + neighbourY * 3 + neighbourZ * 9] = neighbour;
                            }
                        }
                    }
                    
                    // Iterate over all 6 faces of the block
                    for (int face = 0; face < 6; face++)
                    {
                        Vector3i neighbourOffset = NeighbourOffsets[face];
                        BlockState neighbour = _blockStateNeighbourhood[neighbourOffset.X + neighbourOffset.Y * 3 + neighbourOffset.Z * 9];

                        // If the neighbour is opaque, skip this face.
                        // If the neighbour is empty or transparent, we need to mesh this face.
                        if (neighbour.RenderType == BlockRenderType.Normal)
                            continue;

                        // Get the texture index of the block face
                        ushort textureIndex = GetBlockFaceTextureIndex(blockState, (BlockFace)face);
                        
                        // Get the lighting of the block face
                        const int lightLevel = Constants.MAX_LIGHT_LEVEL;
                        const int skyLightLevel = Constants.MAX_LIGHT_LEVEL;
                        Color9 lightColor = Color9.White;
                        
                        // Add the face to the meshing buffer
                        Vector3i blockPos = new(x - 1, y - 1, z - 1);
                        _meshingBuffer.AddFace(_blockStateNeighbourhood, blockPos, (BlockFace)face, textureIndex, lightColor, lightLevel, skyLightLevel);
                    }
                }
            }
        }
        
        chunk.IsMeshDirty = false;
        chunk.IsMeshed = true;

        return _meshingBuffer.CreateMesh(chunkOriginPos);
    }


    private static ushort GetBlockFaceTextureIndex(BlockState block, BlockFace normal)
    {
        return BlockRegistry.GetBlock(block.Id).GetFaceTextureIndex(block, normal);
    }
}