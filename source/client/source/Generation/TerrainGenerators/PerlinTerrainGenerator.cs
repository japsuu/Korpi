using Korpi.Client.Debugging;
using Korpi.Client.Mathematics.Noise;
using Korpi.Client.Registries;
using Korpi.Client.World.Regions.Chunks;
using Korpi.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.Generation.TerrainGenerators;

public class PerlinTerrainGenerator : ITerrainGenerator
{
    private const int SEA_LEVEL = Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 4 + 16;
    private const int TERRAIN_HEIGHT_MIN = SEA_LEVEL - 16;
    private const int TERRAIN_HEIGHT_MAX = SEA_LEVEL + 16;
    
    private readonly FastNoiseLite _noise;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.


    private PerlinTerrainGenerator()
    {
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }


    public static PerlinTerrainGenerator Default()
    {
        return new PerlinTerrainGenerator();
    }


    public void ProcessChunk(in Chunk chunk)
    {
        DebugStats.StartChunkGeneration();
        if (chunk.Bottom > TERRAIN_HEIGHT_MAX)  // Skip chunks above the terrain.
            return;
        
        bool isBelowSurface = chunk.Top < TERRAIN_HEIGHT_MIN;

        BlockState air = BlockRegistry.Air.GetDefaultState();
        BlockState stone = BlockRegistry.GetBlock("korpi:stone").GetDefaultState();
        BlockState dirt = BlockRegistry.GetBlock("korpi:dirt").GetDefaultState();

        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        for (int x = 0; x < Constants.CHUNK_SIZE; x++)
        {
            if (isBelowSurface)
            {
                // Fill the chunk with stone.
                for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                    chunk.SetBlockState(new Vector3i(x, y, z), stone, out _);
                continue;
            }

            int height = GetHeightmapAtPosition(new Vector2i(x + chunk.Position.X, z + chunk.Position.Z));

            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                int worldY = chunk.Position.Y + y;

                if (worldY == height)
                {
                    chunk.SetBlockState(new Vector3i(x, y, z), dirt, out _);
                }
                else if (worldY < height)
                {
                    chunk.SetBlockState(new Vector3i(x, y, z), stone, out _);
                }
                else
                {
                    chunk.SetBlockState(new Vector3i(x, y, z), air, out _);
                }
            }
        }

        DebugStats.StopChunkGeneration();
    }
    
    
    private int GetHeightmapAtPosition(Vector2i blockPosition)
    {
        // Obtain a noise value between 0 and 1.
        float noise = _noise.GetNoise(blockPosition.X, blockPosition.Y);
        float height = noise * 0.5f + 0.5f;
        height = Math.Clamp(height, 0, 1);
        
        // Scale the noise value to the terrain height range.
        height = MathHelper.Lerp(TERRAIN_HEIGHT_MIN, TERRAIN_HEIGHT_MAX, height);
        
        return (int)height;
    }
}