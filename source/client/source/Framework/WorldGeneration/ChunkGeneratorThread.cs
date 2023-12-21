﻿using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.WorldGeneration;

public class ChunkGeneratorThread : ChunkProcessorThread<Vector3i>
{
    private FastNoiseLite _noise = null!;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.


    protected override void InitializeThread()
    {
        base.InitializeThread();
        
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }


    protected override Vector3i ProcessChunk(Chunk chunk)
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
        return chunk.Position;
    }
    
    
    private ushort GetBlockIdAtPosition(Vector3i blockPosition)
    {
        const int seaLevel = Constants.CHUNK_COLUMN_HEIGHT_BLOCKS / 4;
        const int terrainHeightMin = seaLevel - 16;
        const int terrainHeightMax = seaLevel + 16;
        
        return blockPosition.Y > seaLevel ? (ushort)0 : (ushort)1;

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