using Korpi.Client.Debugging;
using Korpi.Client.Mathematics.Noise;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Regions.Chunks;
using Korpi.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.Generation.TerrainGenerators;

[Obsolete("Not implemented yet.")]
public class Simplex3DTerrainGenerator : ITerrainGenerator
{
    private const int TERRAIN_HEIGHT_MAX = 128;
    private const int TERRAIN_HEIGHT_MIN = 72;
    
    private readonly FastNoiseLite _heightmapNoise;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.
    private readonly FastNoiseLite _caveNoise;


    private Simplex3DTerrainGenerator()
    {
        _heightmapNoise = new FastNoiseLite();
        _heightmapNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        
        _caveNoise = new FastNoiseLite();
        _caveNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _caveNoise.SetFractalType(FastNoiseLite.FractalType.FBm); // Fractal Brownian Motion for a rough, natural look
    }


    public static Simplex3DTerrainGenerator Default()
    {
        return new Simplex3DTerrainGenerator();
    }


    public bool WillProcessChunk(Chunk chunk)
    {
        // Skip chunks above the terrain.
        return chunk.Bottom <= TERRAIN_HEIGHT_MAX;
    }


    public void ProcessChunk(in Chunk chunk)
    {
        
        DebugStats.StartChunkGeneration();
        
        BlockState stone = BlockRegistry.GetBlockDefaultState(1);

        DebugStats.StopChunkGeneration();
    }


    private float GetHeightmapAtPosition(Vector2i blockPosition)
    {
        // Obtain a noise value between 0 and 1.
        float noise = _heightmapNoise.GetNoise(blockPosition.X, blockPosition.Y);
        float height = noise * 0.5f + 0.5f;
        height = Math.Clamp(height, 0, 1);
        
        return height;
    }
}