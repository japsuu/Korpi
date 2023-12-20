using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkGeneratorThread : ChunkProcessorThread
{
    private readonly FastNoiseLite _noise;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.


    public ChunkGeneratorThread()
    {
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }


    protected override void ProcessChunk(Chunk chunk)
    {
        for (int z = 0; z < Constants.CHUNK_SIZE; z++)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    ushort id = GetBlockIdAtPosition(new Vector3i(x + chunk.Position.X, y + chunk.Position.Y, z + chunk.Position.Z));
                    
                    chunk.SetBlockState(new Vector3i(x, y, z), BlockRegistry.GetBlock(id).GetDefaultState());
                }
            }
        }
    }
    
    
    private ushort GetBlockIdAtPosition(Vector3i blockPosition)
    {
        const int seaLevel = Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 4;
        const int terrainHeightMin = seaLevel - 16;
        const int terrainHeightMax = seaLevel + 16;
        
        float noise = _noise.GetNoise(blockPosition.X, blockPosition.Z);
        float height = noise * 0.5f + 0.5f;
        
        height = Math.Clamp(height, 0, 1);
        height = MathUtils.Lerp(terrainHeightMin, terrainHeightMax, height);
        
        if (blockPosition.Y > height)
            return 0;
        
        if (blockPosition.Y > height - 2)
            return 2;
        
        return 1;
    }
}