using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Mathematics.Noise;
using Korpi.Client.World.Chunks;
using OpenTK.Mathematics;

namespace Korpi.Client.Generation.TerrainGenerators;

[Obsolete("This terrain generator is not finished yet.")]
public class HeightmapTerrainGenerator : ITerrainGenerator
{
    private const int SEA_LEVEL = Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 4 + 16;
    private const int TERRAIN_HEIGHT_MIN = SEA_LEVEL - 16;
    private const int TERRAIN_HEIGHT_MAX = SEA_LEVEL + 16;
    
    private readonly FastNoiseLite _heightmapNoise;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.
    private readonly FastNoiseLite _caveNoise;


    private HeightmapTerrainGenerator()
    {
        _heightmapNoise = new FastNoiseLite();
        _heightmapNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        
        _caveNoise = new FastNoiseLite();
        _caveNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _caveNoise.SetFractalType(FastNoiseLite.FractalType.FBm); // Fractal Brownian Motion for a rough, natural look
        _caveNoise.SetFractalOctaves(4);
        _caveNoise.SetFractalLacunarity(2);
        _caveNoise.SetFractalGain(0.5f);
        _caveNoise.SetFrequency(0.01f);
        _caveNoise.SetSeed(0);
    }


    public static HeightmapTerrainGenerator Default()
    {
        return new HeightmapTerrainGenerator();
    }


    public void ProcessChunk(in ChunkColumn chunkColumn)
    {
        throw new NotImplementedException();
    }
    
    
    private int GetHeightmapAtPosition(Vector2i blockPosition)
    {
        // Obtain a noise value between 0 and 1.
        float noise = _heightmapNoise.GetNoise(blockPosition.X, blockPosition.Y);
        float height = noise * 0.5f + 0.5f;
        height = Math.Clamp(height, 0, 1);
        
        // Scale the noise value to the terrain height range.
        height = MathHelper.Lerp(TERRAIN_HEIGHT_MIN, TERRAIN_HEIGHT_MAX, height);
        
        return (int)height;
    }
    
    
    private bool GetCaveAtPosition(Vector3i blockPosition)
    {
        // How common caves are, generally a 0-1 value.
        const float caveSize = 0.5f;
        
        // The average height of the cave roof/floor from the caveHeight.
        const float baseCaveHeight = 8;
        
        // The height caves will be the most common at.
        const int caveAverageElevation = TERRAIN_HEIGHT_MIN - 32;
        
        // Obtain a noise value between 0 and 1.
        float rawNoise = Math.Clamp(_caveNoise.GetNoise(blockPosition.X, blockPosition.Y, blockPosition.Z), 0, 1);
        float remappedNoise = rawNoise * 0.5f + 0.5f;

        // Calculate the distance from the current block position to the caveHeight.
        float distanceToCaveHeight = Math.Abs(blockPosition.Y - caveAverageElevation);

        // Use a Gaussian distribution to make the caves most common at the caveHeight and less common as you move away from this height.
        float caveHeight = baseCaveHeight + rawNoise * 64;
        float gaussianFactor = (float)Math.Exp(-Math.Pow(distanceToCaveHeight, 2) / (2 * Math.Pow(caveHeight, 2)));

        // Multiply the noise value by the Gaussian factor to make the caves less common as you move away from the caveHeight.
        float result = remappedNoise * gaussianFactor;

        // Determine whether a cave should be generated at this position.
        return result > caveSize;
    }
}