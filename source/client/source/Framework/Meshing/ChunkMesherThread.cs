using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Framework.WorldGeneration;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

public class ChunkMesherThread : ChunkProcessorThread<ChunkMesh>
{
    /// <summary>
    /// Cache in tyo which the data of the chunk currently being meshed is copied into.
    /// Also includes one block wide border extending into the neighbouring chunks.
    /// </summary>
    private MeshingDataCache _meshingDataCache = null!;
    
    /// <summary>
    /// Buffer into which the meshing thread writes the mesh data.
    /// </summary>
    private MeshingBuffer _meshingBuffer = null!;
    
    /// <summary>
    /// Array containing the block states of the 3x3x3 neighbourhood of the block currently being meshed.
    /// Used to for example calculate the AO of the block faces.
    /// </summary>
    private BlockState[] _blockStateNeighbourhood = null!;
    
    /// <summary>
    /// Offsets of the 6 neighbours of a block.
    /// Used to quickly fetch the block state of a neighbour based on the face normal.
    /// </summary>
    private static readonly Vector3i[] BlockNeighbourOffsets =
    {
        new(2, 1, 1),
        new(1, 2, 1),
        new(1, 1, 2),
        new(0, 1, 1),
        new(1, 0, 1),
        new(1, 1, 0),
    };


    protected override void InitializeThread()
    {
        base.InitializeThread();
        
        _meshingDataCache = new MeshingDataCache(Constants.CHUNK_SIZE);
        _meshingBuffer = new MeshingBuffer();
        _blockStateNeighbourhood = new BlockState[27];   // 27 = 3x3x3
    }


    protected override ChunkMesh ProcessChunk(Chunk chunk)
    {
        RenderingStats.StartChunkMeshing();
        World.CurrentWorld.ChunkManager.FillMeshingCache(chunk.Position, _meshingDataCache);
        
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
                        Vector3i neighbourOffset = BlockNeighbourOffsets[face];
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
                        Vector3i blockPos = new(x - 1, y - 1, z - 1);   // -1 because we started the iteration at 1, since the cache has a border.
                        _meshingBuffer.AddFace(_blockStateNeighbourhood, blockPos, (BlockFace)face, textureIndex, lightColor, lightLevel, skyLightLevel);
                    }
                }
            }
        }
        
        RenderingStats.StopChunkMeshing();

        return _meshingBuffer.CreateMesh(chunk.Position);
    }


    private ushort GetBlockFaceTextureIndex(BlockState block, BlockFace normal)
    {
        return BlockRegistry.GetBlock(block.Id).GetFaceTextureIndex(block, normal);
    }
}