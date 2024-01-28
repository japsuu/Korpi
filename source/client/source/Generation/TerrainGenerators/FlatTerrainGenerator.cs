using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Registries;
using Korpi.Client.World.Chunks;

namespace Korpi.Client.Generation.TerrainGenerators;

public class FlatTerrainGenerator : ITerrainGenerator
{
    private const int TERRAIN_HEIGHT = Constants.SUBCHUNK_SIDE_LENGTH * 4 - 1;
    
    
    private FlatTerrainGenerator()
    {
        
    }


    public static FlatTerrainGenerator Default()
    {
        return new FlatTerrainGenerator();
    }


    public void ProcessChunk(in Chunk subChunk)
    {
        DebugStats.StartChunkGeneration();
        
        BlockState stone = BlockRegistry.GetBlockDefaultState(1);
        
        for (int z = 0; z < Constants.SUBCHUNK_SIDE_LENGTH; z++)
        {
            for (int x = 0; x < Constants.SUBCHUNK_SIDE_LENGTH; x++)
            {
                for (int y = 0; y < Constants.SUBCHUNK_SIDE_LENGTH; y++)
                {
                    int worldY = y + subChunk.Position.Y;
                    
                    if (worldY > TERRAIN_HEIGHT)
                        continue;
                    
                    subChunk.SetBlockState(x, y, z, stone, out _, false);
                }
            }
        }

        DebugStats.StopChunkGeneration();
    }
}