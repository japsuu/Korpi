using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;


public static class ChunkMesher
{
    private const int MAX_CHUNKS_MESHED_PER_FRAME = 8;
    
    /// <summary>
    /// Priority queue of chunks to mesh.
    /// Chunks are prioritized by their distance to the camera.
    /// </summary>
    private static readonly PriorityQueue<Vector3i, float> ChunkMeshingQueue = new();
    private static readonly HashSet<Vector3i> QueuedChunks = new();

    /// <summary>
    /// Data of the chunk currently being meshed.
    /// Also includes one block wide border extending into the neighbouring chunks.
    /// </summary>
    private static readonly MeshingDataCache MeshingDataCache = new(Constants.CHUNK_SIZE);
    private static readonly MeshingBuffer MeshingBuffer = new();
    private static readonly BlockState[] BlockStateNeighbourhood = new BlockState[27];   // 27 = 3x3x3
    
    private static readonly Vector3i[] NeighbourOffsets =
    {
        new(2, 1, 1),
        new(1, 2, 1),
        new(1, 1, 2),
        new(0, 1, 1),
        new(1, 0, 1),
        new(1, 1, 0),
    };


    public static void ProcessMeshingQueue()
    {
        RenderingStats.ChunksInMeshingQueue = (ulong)ChunkMeshingQueue.Count;
        RenderingStats.StartMeshing();
        
        int chunksMeshed = 0;
        while (chunksMeshed < MAX_CHUNKS_MESHED_PER_FRAME && ChunkMeshingQueue.Count > 0)
        {
            Vector3i chunkPos = ChunkMeshingQueue.Dequeue();
            QueuedChunks.Remove(chunkPos);
            
            // Check if the chunk is still loaded
            if (!World.CurrentWorld.ChunkManager.IsChunkLoaded(chunkPos))
                continue;
            
            RenderingStats.StartChunkMeshing();
            ChunkRenderer mesh = GenerateMesh(chunkPos);
            ChunkRendererStorage.AddRenderer(chunkPos, mesh);
            chunksMeshed++;
            RenderingStats.StopChunkMeshing();
        }
        
        RenderingStats.StopMeshing(chunksMeshed);
    }


    public static void EnqueueChunkForInitialMeshing(Vector3i chunkOriginPos)
    {
        if (QueuedChunks.Contains(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already queued!");

        if (ChunkRendererStorage.ContainsRenderer(chunkOriginPos))
            throw new InvalidOperationException($"Tried to mesh chunk at {chunkOriginPos} that is already meshed!");

        float distanceToPlayer = (chunkOriginPos - PlayerEntity.LocalPlayerEntity.Transform.LocalPosition).LengthSquared;
        ChunkMeshingQueue.Enqueue(chunkOriginPos, distanceToPlayer);
        QueuedChunks.Add(chunkOriginPos);
    }


    public static void EnqueueChunkForMeshing(Vector3i chunkOriginPos)
    {
        if (QueuedChunks.Contains(chunkOriginPos))
            return;

        if (ChunkRendererStorage.ContainsRenderer(chunkOriginPos))
        {
            Logger.LogWarning($"Trashing the mesh of chunk at {chunkOriginPos}!");
            ChunkRendererStorage.RemoveRenderer(chunkOriginPos);
        }
        
        float distanceToCamera = (chunkOriginPos - PlayerEntity.LocalPlayerEntity.Transform.LocalPosition).LengthSquared;
        ChunkMeshingQueue.Enqueue(chunkOriginPos, distanceToCamera);
        QueuedChunks.Add(chunkOriginPos);
    }


    private static ChunkRenderer GenerateMesh(Vector3i chunkOriginPos)
    {
        Chunk chunk = World.CurrentWorld.ChunkManager.GetChunkAndFillMeshingCache(chunkOriginPos, MeshingDataCache);
        
        MeshingBuffer.Clear();
        
        // Mesh the chunk based on the data cache, using _meshingBuffer.AddFace(blockPos, faceNormal, textureIndex, lighting) to add faces to the buffer.
        // Exploit the meshing cache's spatial locality by iterating over the blocks in the order they are stored in the cache.
        for (int z = 1; z <= Constants.CHUNK_SIZE; z++)     // Start at 1 on all axis to skip the border.
        {
            for (int y = 1; y <= Constants.CHUNK_SIZE; y++)
            {
                for (int x = 1; x <= Constants.CHUNK_SIZE; x++)
                {
                    BlockState blockState = MeshingDataCache.GetData(x, y, z);
                    
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
                                BlockState neighbour = MeshingDataCache.GetData(x + neighbourX - 1, y + neighbourY - 1, z + neighbourZ - 1);
                                BlockStateNeighbourhood[neighbourX + neighbourY * 3 + neighbourZ * 9] = neighbour;
                            }
                        }
                    }
                    
                    // Iterate over all 6 faces of the block
                    for (int face = 0; face < 6; face++)
                    {
                        Vector3i neighbourOffset = NeighbourOffsets[face];
                        BlockState neighbour = BlockStateNeighbourhood[neighbourOffset.X + neighbourOffset.Y * 3 + neighbourOffset.Z * 9];

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
                        MeshingBuffer.AddFace(BlockStateNeighbourhood, blockPos, (BlockFace)face, textureIndex, lightColor, lightLevel, skyLightLevel);
                    }
                }
            }
        }
        
        chunk.OnMeshed();

        return MeshingBuffer.CreateMesh(chunkOriginPos);
    }


    private static ushort GetBlockFaceTextureIndex(BlockState block, BlockFace normal)
    {
        return BlockRegistry.GetBlock(block.Id).GetFaceTextureIndex(block, normal);
    }
}