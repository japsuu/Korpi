using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Mathematics.Noise;
using Korpi.Client.Registries;
using Korpi.Client.World.Chunks;
using Korpi.Client.World.Chunks.Blocks;

namespace Korpi.Client.Generation.TerrainGenerators;

/// <summary>
/// Generates noise-based terrain by splitting the chunk into 4x4x4 "bricks", sampling noise at each brick's corner, and then interpolating the values.
/// </summary>
public class Simplex3DTerrainGenerator : ITerrainGenerator
{
    private const int NOISE_SAMPLE_BRICK_SIZE = 4;
    private const int BRICK_COUNT_H = Constants.SUBCHUNK_SIDE_LENGTH / NOISE_SAMPLE_BRICK_SIZE;
    private const int BRICK_COUNT_V = Constants.CHUNK_HEIGHT_BLOCKS / NOISE_SAMPLE_BRICK_SIZE;
    
    private const int MIN_ALLOWED_TERRAIN_HEIGHT = Constants.SUBCHUNK_SIDE_LENGTH;
    private const int MAX_ALLOWED_TERRAIN_HEIGHT = Constants.CHUNK_HEIGHT_BLOCKS - Constants.SUBCHUNK_SIDE_LENGTH;

    private float _terrainBaseHeight = 256;          // The base height of the terrain, in blocks.
    private float _terrainHeightMaxOffset = 64;    // The maximum how much the terrain height may be offset from the base height, in blocks.
    private float _minSquash = 0.5f;
    private float _maxSquash = 8f;

    private readonly FastNoiseLite _densityNoise; // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.
    private readonly FastNoiseLite _squashNoise;
    private readonly FastNoiseLite _heightNoise;
    
    
    private void SetMinSquash(float value)
    {
        _minSquash = value;
        _maxSquash = Math.Max(_maxSquash, value);
    }
    
    
    private void SetMaxSquash(float value)
    {
        _maxSquash = value;
        _minSquash = Math.Min(_minSquash, value);
    }


    private Simplex3DTerrainGenerator()
    {
        _densityNoise = new FastNoiseLite();
        _densityNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _densityNoise.SetFractalOctaves(3);
        _densityNoise.SetFractalLacunarity(2);
        _densityNoise.SetFractalGain(0.5f);
        _densityNoise.SetFrequency(0.01f);
        _densityNoise.SetSeed(0);
        
        _squashNoise = new FastNoiseLite();
        _squashNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _squashNoise.SetFrequency(0.005f);
        
        _heightNoise = new FastNoiseLite();
        _heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _heightNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        _densityNoise.SetFractalOctaves(3);
        _densityNoise.SetFractalLacunarity(2);
        _densityNoise.SetFractalGain(0.5f);
        _heightNoise.SetFrequency(0.002f);

#if DEBUG
        UI.Windows.GenerationWindow.RegisterNoise("Density", _densityNoise);
        UI.Windows.GenerationWindow.RegisterNoise("Squash", _squashNoise);
        UI.Windows.GenerationWindow.RegisterNoise("Height", _heightNoise);
        UI.Windows.GenerationWindow.RegisterVariable(new UI.Windows.Variables.EditorVariableFloat("Terrain Base Height", value => _terrainBaseHeight = value, () => _terrainBaseHeight, MIN_ALLOWED_TERRAIN_HEIGHT, MAX_ALLOWED_TERRAIN_HEIGHT));
        UI.Windows.GenerationWindow.RegisterVariable(new UI.Windows.Variables.EditorVariableFloat("Terrain Height Max Offset", value => _terrainHeightMaxOffset = value, () => _terrainHeightMaxOffset, 0, 128));
        UI.Windows.GenerationWindow.RegisterVariable(new UI.Windows.Variables.EditorVariableFloat("Min Squash", SetMinSquash, () => _minSquash, 0, 100));
        UI.Windows.GenerationWindow.RegisterVariable(new UI.Windows.Variables.EditorVariableFloat("Max Squash", SetMaxSquash, () => _maxSquash, 0, 100));
#endif
    }


    public static Simplex3DTerrainGenerator Default()
    {
        return new Simplex3DTerrainGenerator();
    }


    public void ProcessChunk(in Chunk chunk)
    {
        DebugStats.StartChunkGeneration();

        if (chunk.Position.X < 7 * Constants.SUBCHUNK_SIDE_LENGTH)
            return;

        BlockState stone = BlockRegistry.GetBlockDefaultState(1);

        // Process each brick in the chunk.
        for (int brickZ = 0; brickZ < BRICK_COUNT_H; brickZ++)
        {
            int brickZChunk = brickZ * NOISE_SAMPLE_BRICK_SIZE;
            int brickWorldZ = chunk.Position.Y + brickZChunk;
            for (int brickY = 0; brickY < BRICK_COUNT_V; brickY++)
            {
                int brickWorldY = brickY * NOISE_SAMPLE_BRICK_SIZE;
                for (int brickX = 0; brickX < BRICK_COUNT_H; brickX++)
                {
                    int brickXChunk = brickX * NOISE_SAMPLE_BRICK_SIZE;
                    int brickWorldX = chunk.Position.X + brickXChunk;

                    // Sample the noise at each corner of the brick.
                    int brickWorldXExt = brickWorldX + NOISE_SAMPLE_BRICK_SIZE;
                    int brickWorldYExt = brickWorldY + NOISE_SAMPLE_BRICK_SIZE;
                    int brickWorldZExt = brickWorldZ + NOISE_SAMPLE_BRICK_SIZE;
                    float density000 = _densityNoise.GetNoise(brickWorldX, brickWorldY, brickWorldZ);
                    float density100 = _densityNoise.GetNoise(brickWorldXExt, brickWorldY, brickWorldZ);
                    float density010 = _densityNoise.GetNoise(brickWorldX, brickWorldYExt, brickWorldZ);
                    float density110 = _densityNoise.GetNoise(brickWorldXExt, brickWorldYExt, brickWorldZ);
                    float density001 = _densityNoise.GetNoise(brickWorldX, brickWorldY, brickWorldZExt);
                    float density101 = _densityNoise.GetNoise(brickWorldXExt, brickWorldY, brickWorldZExt);
                    float density011 = _densityNoise.GetNoise(brickWorldX, brickWorldYExt, brickWorldZExt);
                    float density111 = _densityNoise.GetNoise(brickWorldXExt, brickWorldYExt, brickWorldZExt);
                    float height000 = _heightNoise.GetNoise(brickWorldX, brickWorldY, brickWorldZ);
                    float height100 = _heightNoise.GetNoise(brickWorldXExt, brickWorldY, brickWorldZ);
                    float height010 = _heightNoise.GetNoise(brickWorldX, brickWorldYExt, brickWorldZ);
                    float height110 = _heightNoise.GetNoise(brickWorldXExt, brickWorldYExt, brickWorldZ);
                    float height001 = _heightNoise.GetNoise(brickWorldX, brickWorldY, brickWorldZExt);
                    float height101 = _heightNoise.GetNoise(brickWorldXExt, brickWorldY, brickWorldZExt);
                    float height011 = _heightNoise.GetNoise(brickWorldX, brickWorldYExt, brickWorldZExt);
                    float height111 = _heightNoise.GetNoise(brickWorldXExt, brickWorldYExt, brickWorldZExt);
                    float squash000 = _squashNoise.GetNoise(brickWorldX, brickWorldY, brickWorldZ);
                    float squash100 = _squashNoise.GetNoise(brickWorldXExt, brickWorldY, brickWorldZ);
                    float squash010 = _squashNoise.GetNoise(brickWorldX, brickWorldYExt, brickWorldZ);
                    float squash110 = _squashNoise.GetNoise(brickWorldXExt, brickWorldYExt, brickWorldZ);
                    float squash001 = _squashNoise.GetNoise(brickWorldX, brickWorldY, brickWorldZExt);
                    float squash101 = _squashNoise.GetNoise(brickWorldXExt, brickWorldY, brickWorldZExt);
                    float squash011 = _squashNoise.GetNoise(brickWorldX, brickWorldYExt, brickWorldZExt);
                    float squash111 = _squashNoise.GetNoise(brickWorldXExt, brickWorldYExt, brickWorldZExt);

                    // Process each block within the brick.
                    for (int blockZ = 0; blockZ < NOISE_SAMPLE_BRICK_SIZE; blockZ++)
                    {
                        int worldZ = brickWorldZ + blockZ;
                        for (int blockY = 0; blockY < NOISE_SAMPLE_BRICK_SIZE; blockY++)
                        {
                            int worldY = brickWorldY + blockY;
                            for (int blockX = 0; blockX < NOISE_SAMPLE_BRICK_SIZE; blockX++)
                            {
                                int worldX = brickWorldX + blockX;
                                // Calculate the relative position of the block within its brick.
                                float rx = blockX / (float)NOISE_SAMPLE_BRICK_SIZE;
                                float ry = blockY / (float)NOISE_SAMPLE_BRICK_SIZE;
                                float rz = blockZ / (float)NOISE_SAMPLE_BRICK_SIZE;

                                // Interpolate the noise value for the block's position.
                                float density = TrilinearInterpolate(
                                    density000,
                                    density100,
                                    density010,
                                    density110,
                                    density001,
                                    density101,
                                    density011,
                                    density111,
                                    rx, ry, rz);
                                float height = TrilinearInterpolate(
                                    height000,
                                    height100,
                                    height010,
                                    height110,
                                    height001,
                                    height101,
                                    height011,
                                    height111,
                                    rx, ry, rz);
                                float squash = TrilinearInterpolate(
                                    squash000,
                                    squash100,
                                    squash010,
                                    squash110,
                                    squash001,
                                    squash101,
                                    squash011,
                                    squash111,
                                    rx, ry, rz);
                                
                                int chunkX = brickXChunk + blockX;
                                int chunkY = brickWorldY + blockY;
                                int chunkZ = brickZChunk + blockZ;

                                GenerateTerrain(chunk, worldX, worldY, worldZ, chunkX, chunkY, chunkZ, density, height, squash, stone);
                            }
                        }
                    }
                }
            }
        }
        
        DecorateSurface(chunk);

        DebugStats.StopChunkGeneration();
    }


    /// <summary>
    /// Tries to generate terrain at the given position.
    /// The terrain generation is controlled by two parameters: squashing factor and terrain height.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="worldX">X position of the block in world space.</param>
    /// <param name="worldY">Y position of the block in world space.</param>
    /// <param name="worldZ">Z position of the block in world space.</param>
    /// <param name="chunkX"></param>
    /// <param name="chunkY"></param>
    /// <param name="chunkZ"></param>
    /// <param name="densityNoise">The value of the density noise field at the given position. Bounded between -1 and 1.</param>
    /// <param name="heightNoise">The value of the height noise field at the given position. Bounded between -1 and 1.</param>
    /// <param name="squashNoise">The value of the squash noise field at the given position. Bounded between -1 and 1.</param>
    /// <param name="stone"></param>
    private void GenerateTerrain(Chunk chunk, int worldX, int worldY, int worldZ, int chunkX, int chunkY, int chunkZ, float densityNoise, float heightNoise, float squashNoise, BlockState stone)
    {
        // Get average terrain height at this point, in blocks.
        float averageTerrainHeight = GetTerrainHeight(heightNoise);

        // Calculate the difference between the current block's height and the average terrain height.
        float heightDifference = worldY - averageTerrainHeight;

        // Normalize the height difference to a range of -1 to 1.
        float normalizedHeightDifference = heightDifference / _terrainHeightMaxOffset;

        // Multiply the normalized height difference by the squash factor.
        float squashFactor = normalizedHeightDifference * GetSquash(squashNoise);

        // Subtract the squash factor from the density.
        densityNoise -= squashFactor;

        // Place terrain if density is above 0.
        if (densityNoise > 0)
        {
            chunk.SetBlockState(chunkX, chunkY, chunkZ, stone, out _, false);
        }
    }


    private void DecorateSurface(Chunk chunk)
    {
        BlockState dirtBlockState = BlockRegistry.GetBlockDefaultState(2);
        
        for (int x = 0; x < Constants.SUBCHUNK_SIDE_LENGTH; x++)
        {
            for (int z = 0; z < Constants.SUBCHUNK_SIDE_LENGTH; z++)
            {
                int surfaceBlocksSet = 0;
                int encounteredSolidBlocks = 0;
                for (int y = chunk.GetHighestBlock(x, z); y >= 0; y--)
                {
                    BlockState blockState = chunk.GetBlockState(x, y, z);

                    if (blockState.IsAir)
                    {
                        surfaceBlocksSet = 0;
                        continue;
                    }
                    encounteredSolidBlocks++;
                    
                    if (surfaceBlocksSet >= 3)
                        continue;

                    chunk.SetBlockState(x, y, z, dirtBlockState, out _, false);

                    surfaceBlocksSet++;
                    if (encounteredSolidBlocks > 10)
                        break;
                }
            }
        }
    }


    private float GetTerrainHeight(float noise)
    {
        float heightFactor = noise; //TODO: Implement with curves.
        
        return _terrainBaseHeight + heightFactor * _terrainHeightMaxOffset;
    }


    private float GetSquash(float squash)
    {
        squash = (squash + 1) / 2; // squash is now between 0 and 1
        squash = squash * (_maxSquash - _minSquash) + _minSquash; // squash is now between minSquash and maxSquash
        return squash;
    }


    private static float TrilinearInterpolate(float c000, float c100, float c010, float c110, float c001, float c101, float c011, float c111, float x, float y, float z)
    {
        float c00 = c000 * (1 - x) + c100 * x;
        float c01 = c001 * (1 - x) + c101 * x;
        float c10 = c010 * (1 - x) + c110 * x;
        float c11 = c011 * (1 - x) + c111 * x;

        float c0 = c00 * (1 - y) + c10 * y;
        float c1 = c01 * (1 - y) + c11 * y;

        return c0 * (1 - z) + c1 * z;
    }
}