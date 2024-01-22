using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Registries;
using Korpi.Client.World;
using Korpi.Client.World.Regions.Chunks;
using Korpi.Client.World.Regions.Chunks.Blocks;

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


    public bool WillProcessChunk(Chunk chunk)
    {
        // Skip chunks above the terrain.
        return chunk.Bottom <= TERRAIN_HEIGHT;
    }


    public void ProcessChunk(in Chunk chunk)
    {
        DebugStats.StartChunkGeneration();
        
        BlockState stone = BlockRegistry.GetBlockDefaultState(1);
        
        for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
        {
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
            {
                for (int y = 0; y < Constants.CHUNK_SIDE_LENGTH; y++)
                {
                    int worldY = y + chunk.Position.Y;
                    
                    if (worldY > TERRAIN_HEIGHT)
                        continue;
                    
                    ChunkBlockPosition pos = new(x, y, z);
                    chunk.SetBlockState(pos, stone, out _, false);
                }
            }
        }

        DebugStats.StopChunkGeneration();
    }
}