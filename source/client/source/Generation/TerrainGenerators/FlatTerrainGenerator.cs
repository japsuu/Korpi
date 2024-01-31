using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Registries;
using Korpi.Client.World.Chunks;

namespace Korpi.Client.Generation.TerrainGenerators;

public class FlatTerrainGenerator : ITerrainGenerator
{
    private const int TERRAIN_HEIGHT = Constants.CHUNK_SIDE_LENGTH * 4 - 1;
    
    
    private FlatTerrainGenerator()
    {
        
    }


    public static FlatTerrainGenerator Default()
    {
        return new FlatTerrainGenerator();
    }


    public void ProcessChunk(in ChunkColumn chunkColumn)
    {
        DebugStats.StartChunkGeneration();
        
        BlockState stone = BlockRegistry.GetBlockDefaultState(1);
        
        for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
        {
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
            {
                for (int y = 0; y < Constants.CHUNK_COLUMN_HEIGHT_BLOCKS; y++)
                {
                    if (y > TERRAIN_HEIGHT)
                        continue;
                    
                    chunkColumn.SetBlockState(x, y, z, stone, out _, false);
                }
            }
        }

        DebugStats.StopChunkGeneration();
    }
}