using System.Collections.Concurrent;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Utils;
using ConcurrentCollections;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkGeneratorThread : IDisposable
{
    private volatile bool _shouldStop;
    private readonly Thread _thread;
    private readonly FastNoiseLite _noise;   // FNL seems to be thread safe, as long as you don't change the seed/other settings while generating.
    private readonly ConcurrentHashSet<Vector3i> _queuedChunks;
    private readonly ConcurrentQueue<Vector3i> _inputQueue;


    public ChunkGeneratorThread(ConcurrentHashSet<Vector3i> queuedChunks, ConcurrentQueue<Vector3i> inputQueue)
    {
        _queuedChunks = queuedChunks;
        _inputQueue = inputQueue;
        
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        
        _thread = new Thread(GenerateChunks);
        _thread.Start();
    }


    private void GenerateChunks()
    {
        
        while (!_shouldStop)
        {
            if (_inputQueue.TryDequeue(out Vector3i chunkPos))
            {
                Chunk? chunk = World.CurrentWorld.ChunkManager.GetChunkAt(chunkPos);
                if (chunk == null)
                    continue;
                
                GenerateChunk(chunk);
                _queuedChunks.TryRemove(chunkPos);
            }
        }
    }
    
    
    private void GenerateChunk(Chunk chunk)
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
        
        chunk.OnGenerated();
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


    private void ReleaseUnmanagedResources()
    {
        _shouldStop = true;
        _thread.Join();
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~ChunkGeneratorThread()
    {
        ReleaseUnmanagedResources();
    }
}