using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Framework.Threading;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.WorldGeneration;

public class ChunkGeneratorThread : ChunkProcessorThread<Vector3i>
{
    private const int SEA_LEVEL = Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 4 + 16;
    private const int TERRAIN_HEIGHT_MIN = SEA_LEVEL - 16;
    private const int TERRAIN_HEIGHT_MAX = SEA_LEVEL + 16;
    
    private FastNoiseLite _noise = null!;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.


    protected override void InitializeThread()
    {
        base.InitializeThread();
        
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }


    protected override Vector3i ProcessChunk(Chunk chunk)
    {
        RenderingStats.StartChunkGeneration();
        if (chunk.Bottom > TERRAIN_HEIGHT_MAX)  // Skip chunks above the terrain.
            return chunk.Position;
        
        bool isBelowSurface = chunk.Top < TERRAIN_HEIGHT_MIN;

        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        for (int x = 0; x < Constants.CHUNK_SIZE; x++)
        {
            if (isBelowSurface)
            {
                // Fill the chunk with stone.
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    chunk.SetBlockState(new Vector3i(x, y, z), BlockRegistry.GetBlock(1).GetDefaultState(), out _);
                continue;
            }

            int height = GetHeightmapAtPosition(new Vector2i(x + chunk.Position.X, z + chunk.Position.Z));

            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                int worldY = chunk.Position.Y + y;
                ushort blockId = 0; // Default to air.

                if (worldY == height)
                {
                    blockId = 2;
                }
                else if (worldY < height)
                {
                     blockId = 1;
                }
                chunk.SetBlockState(new Vector3i(x, y, z), BlockRegistry.GetBlock(blockId).GetDefaultState(), out _);
            }
        }

        RenderingStats.StopChunkGeneration();
        return chunk.Position;
    }
    
    
    private int GetHeightmapAtPosition(Vector2i blockPosition)
    {
        // Obtain a noise value between 0 and 1.
        float noise = _noise.GetNoise(blockPosition.X, blockPosition.Y);
        float height = noise * 0.5f + 0.5f;
        height = Math.Clamp(height, 0, 1);
        
        // Scale the noise value to the terrain height range.
        height = MathUtils.Lerp(TERRAIN_HEIGHT_MIN, TERRAIN_HEIGHT_MAX, height);
        
        return (int)height;
    }
}