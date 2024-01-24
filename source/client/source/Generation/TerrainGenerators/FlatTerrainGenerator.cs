using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Chunks;
using Korpi.Client.World.Chunks.Blocks;

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


    public bool WillProcessChunk(SubChunk subChunk)
    {
        // Skip chunks above the terrain.
        return subChunk.Bottom <= TERRAIN_HEIGHT;
    }


    public void ProcessChunk(in SubChunk subChunk)
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
                    
                    SubChunkBlockPosition pos = new(x, y, z);
                    subChunk.SetBlockState(pos, stone, out _, false);
                }
            }
        }

        DebugStats.StopChunkGeneration();
    }
}