using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Debugging;
using BlockEngine.Framework.Meshing;
using BlockEngine.Framework.Registries;
using BlockEngine.Framework.Rendering.ImGuiWindows;
using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Chunks;

public class ChunkManager
{
    /// <summary>
    /// Contains all 26 neighbouring chunk offsets.
    /// </summary>
    private static readonly Vector3i[] NeighbouringChunkOffsets = {
        // 8 Corners
        new (1, 1, 1),
        new (-1, 1, 1),
        new (-1, 1, -1),
        new (1, 1, -1),
        new (1, -1, 1),
        new (-1, -1, 1),
        new (-1, -1, -1),
        new (1, -1, -1),
        
        // 12 Edges
        new (1, 1, 0),
        new (0, 1, 1),
        new (-1, 1, 0),
        new (0, 1, -1),
        new (1, -1, 0),
        new (0, -1, 1),
        new (-1, -1, 0),
        new (0, -1, -1),
        new (1, 0, 1),
        new (-1, 0, 1),
        new (-1, 0, -1),
        new (1, 0, -1),
        
        // 6 Faces
        new (1, 0, 0),
        new (0, 0, 1),
        new (-1, 0, 0),
        new (0, 0, -1),
        new (0, 1, 0),
        new (0, -1, 0)
    };
    
    private static readonly NeighbouringChunkPosition[] NeighbouringChunkPositions = Enum.GetValues(typeof(NeighbouringChunkPosition)).Cast<NeighbouringChunkPosition>().ToArray();
    
    private readonly Vector3i[] _precomputedNeighbouringChunkOffsets = new Vector3i[26];
    private readonly Dictionary<Vector2i, ChunkColumn> _loadedColumns = new();
    private readonly List<Vector2i> _columnsToLoad = new();
    private readonly List<Vector2i> _columnsToUnload = new();

    private readonly ChunkMesher _chunkMesher;

    // Precomputed spiral of chunk column positions to load.
    // The spiral is centered around the origin.
    private List<Vector2i> _columnLoadSpiral = null!;


    public ChunkManager()
    {
        _chunkMesher = new ChunkMesher(this);
        PrecomputeNeighbouringChunkOffsets();
        PrecomputeColumnLoadSpiral();
    }


    public void Tick(Vector3 cameraPos, double deltaTime)
    {
        FindColumnsToUnload(cameraPos);
        UnloadColumns();
        FindColumnsToLoad(cameraPos);
        LoadColumns(cameraPos);
        
        // Tick all columns
        foreach (ChunkColumn column in _loadedColumns.Values)
        {
            column.Tick(deltaTime);
            
            if (!column.IsMeshDirty)
                continue;
            
            for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
            {
                Chunk? chunk = column.GetChunk(i);
                if (chunk == null)
                    continue;
                
                if (!chunk.IsMeshed)
                    continue;
                
                if (!chunk.IsMeshDirty)
                    continue;
                
                Vector3i chunkPos = new(column.Position.X, i * Constants.CHUNK_SIZE, column.Position.Y);

                _chunkMesher.EnqueueChunkForMeshing(chunkPos, cameraPos);
                // Logger.Debug($"Enqueued chunk at {chunkPos} for meshing, as it was dirty.");
            }
        }
        
        _chunkMesher.ProcessMeshingQueue();
        RenderingWindow.RenderingStats.LoadedColumnCount = (ulong)_loadedColumns.Count;
    }


    public void Draw(Vector3 cameraPos, Shader chunkShader)
    {
        foreach (ChunkColumn column in _loadedColumns.Values)
        {
            for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
            {
                Chunk? chunk = column.GetChunk(i);
                if (chunk == null)
                    continue;
                
                if (!chunk.IsMeshed)
                    continue;
                
                Vector3i chunkPos = new(column.Position.X, i * Constants.CHUNK_SIZE, column.Position.Y);
                DrawChunkAt(chunkPos, chunkShader);
            }
        }

        if (DebugSettings.RenderChunkBorders)
        {
            // Get the chunk the player is currently in
            Vector3i chunkPos = CoordinateConversions.GetContainingChunkPos(cameraPos);
            DebugChunkDrawer.DrawChunkBorders(chunkPos);
        }

        if (DebugSettings.RenderChunkColumnBorders)
        {
            // Get the chunk the player is currently in
            Vector2i columnPos = CoordinateConversions.GetContainingColumnPos(cameraPos);
            DebugChunkDrawer.DrawChunkColumnBorders(columnPos);
        }
    }


    /// <summary>
    /// Fills the given array with data of the chunk at the given position.
    /// The array also contains 1 block wide slices of the neighbouring chunks.
    /// </summary>
    /// <param name="chunkOriginPos">Position of the center chunk</param>
    /// <param name="cache">Array to fill with BlockState data</param>
    /// <param name="centerChunk">The chunk that is in the center of the cache</param>
    /// <returns>If the center chunk has been generated.</returns>
    public bool FillMeshingCache(Vector3i chunkOriginPos, MeshingDataCache cache, out Chunk? centerChunk)
    {
        Array.Fill(cache.Data, BlockRegistry.Air.GetDefaultState());
        
        centerChunk = GetChunkAt(chunkOriginPos);

        if (centerChunk == null)
            return false;

        // Copy the block data of the center chunk
        centerChunk.CacheMeshingData(cache);

        // Copy the block data of the chunks surrounding the center chunk, but call GetChunk only once for each neighbouring chunk
        for (int i = 0; i < _precomputedNeighbouringChunkOffsets.Length; i++)
        {
            Vector3i neighbourOffset = _precomputedNeighbouringChunkOffsets[i];
            Vector3i neighbourPos = chunkOriginPos + neighbourOffset;
            Chunk? neighbourChunk = GetChunkAt(neighbourPos);

            if (neighbourChunk == null)
                continue;
            
            NeighbouringChunkPosition position = NeighbouringChunkPositions[i];

            // Copy the slice of block data from the neighbour chunk
            neighbourChunk.CacheMeshingData(cache, position);
        }
        
        return true;
    }


    private void FindColumnsToUnload(Vector3 cameraPos)
    {
        _columnsToUnload.Clear();
        Vector2i originColumnPos = CoordinateConversions.GetContainingColumnPos(cameraPos);
        foreach (KeyValuePair<Vector2i, ChunkColumn> pair in _loadedColumns)
        {
            Vector2i normalizedColumnPos = (pair.Key - originColumnPos) / Constants.CHUNK_SIZE;
            bool inRange = normalizedColumnPos.X * normalizedColumnPos.X + normalizedColumnPos.Y * normalizedColumnPos.Y <=
                           Constants.CHUNK_COLUMN_UNLOAD_RADIUS_SQUARED;
            if (inRange)
                continue;
            ChunkColumn column = pair.Value;
            if (column.ReadyToUnload())
                _columnsToUnload.Add(pair.Key);
        }
    }


    private void UnloadColumns()
    {
        foreach (Vector2i columnPos in _columnsToUnload)
        {
            ChunkColumn column = _loadedColumns[columnPos];
            _loadedColumns[columnPos].Unload();
            _loadedColumns.Remove(columnPos);
            for (int y = 0; y < Constants.CHUNK_COLUMN_HEIGHT; y++)
            {
                Chunk? chunk = column.GetChunk(y);
                if (chunk == null)
                    continue;

                Vector3i chunkPos = new(column.Position.X, y * Constants.CHUNK_SIZE, column.Position.Y);
                _chunkMesher.DeleteChunkMesh(chunkPos);
            }
            // TODO: Check if this chunk was queued for meshing, and remove it from the queue.

            // Logger.Log($"Unloaded chunk column at {columnPos}.");
        }

        if (_columnsToUnload.Count > 0)
            Logger.Log($"Unloaded {_columnsToUnload.Count * Constants.CHUNK_COLUMN_HEIGHT} chunks.");
    }


    private void FindColumnsToLoad(Vector3 cameraPos)
    {
        _columnsToLoad.Clear();

        // Get the column position where the loading should start
        Vector2i originColumnPos = CoordinateConversions.GetContainingColumnPos(cameraPos);

        // Load columns in a square around the origin column in a spiral pattern.
        foreach (Vector2i spiralPos in _columnLoadSpiral)
        {
            Vector2i columnPos = originColumnPos + spiralPos;
            if (_loadedColumns.ContainsKey(columnPos))
                continue;

            _columnsToLoad.Add(columnPos);
        }
    }


    private void LoadColumns(Vector3 cameraPos)
    {
        foreach (Vector2i columnPos in _columnsToLoad)
        {
            ChunkColumn column = new(columnPos);
            column.Load();
            EnqueueColumnForInitialMeshing(column, cameraPos);
            _loadedColumns.Add(columnPos, column);

            // Logger.Log($"Loaded chunk column at {columnPos}.");
        }

        if (_columnsToLoad.Count > 0)
            Logger.Log($"Loaded {_columnsToLoad.Count * Constants.CHUNK_COLUMN_HEIGHT} chunks.");
    }


    private void EnqueueColumnForInitialMeshing(ChunkColumn column, Vector3 cameraPos)
    {
        for (int y = 0; y < Constants.CHUNK_COLUMN_HEIGHT; y++)
        {
            Chunk? chunk = column.GetChunk(y);
            if (chunk == null)
                continue;

            Vector3i chunkPos = new(column.Position.X, y * Constants.CHUNK_SIZE, column.Position.Y); 
            _chunkMesher.EnqueueChunkForInitialMeshing(chunkPos, cameraPos);
        }
        // Logger.Debug($"Enqueued column at {column.Position} for full meshing.");
    }


    private Chunk? GetChunkAt(Vector3i position)
    {
        Vector2i chunkColumnPos = CoordinateConversions.GetContainingColumnPos(position);

        if (!_loadedColumns.TryGetValue(chunkColumnPos, out ChunkColumn? column))
        {
            //Logger.LogWarning($"Tried to get unloaded ChunkColumn at {position} ({chunkColumnPos})!");
            return null;
        }

        return column.GetChunkAtHeight(position.Y);
    }


    private void DrawChunkAt(Vector3i position, Shader chunkShader)
    {
        if (ChunkRendererStorage.TryGetRenderer(position, out ChunkRenderer? mesh))
            mesh!.Draw(chunkShader);
    }


    private void PrecomputeNeighbouringChunkOffsets()
    {
        for (int i = 0; i < NeighbouringChunkOffsets.Length; i++)
        {
            Vector3i offset = NeighbouringChunkOffsets[i];
            NeighbouringChunkPosition position = NeighbouringChunkPositions[i];
            _precomputedNeighbouringChunkOffsets[(int)position] = offset * Constants.CHUNK_SIZE;
        }

        Logger.Log($"Precomputed chunk offsets for {_precomputedNeighbouringChunkOffsets.Length} neighbouring chunks.");
    }


    private void PrecomputeColumnLoadSpiral()
    {
        const int size = Constants.CHUNK_COLUMN_LOAD_RADIUS * 2 + 1;
        _columnLoadSpiral = new List<Vector2i>
        {
            new Vector2i(0, 0)
        };

        foreach (Vector2i pos in EnumerateSpiral(size * size - 1))
        {
            bool inRange = pos.X * pos.X + pos.Y * pos.Y <= Constants.CHUNK_COLUMN_LOAD_RADIUS_SQUARED;

            // Ensure that the position is inside the load radius
            if (!inRange)
                continue;
            _columnLoadSpiral.Add(pos * Constants.CHUNK_SIZE);
        }

        Logger.Log($"Precomputed column load spiral for render distance {Constants.CHUNK_COLUMN_LOAD_RADIUS}, for {_columnLoadSpiral.Count} columns.");
    }


    private static IEnumerable<Vector2i> EnumerateSpiral(int size)
    {
        int di = 1;
        int dj = 0;
        int segmentLength = 1;

        int i = 0;
        int j = 0;
        int segmentPassed = 0;
        for (int k = 0; k < size; ++k)
        {
            i += di;
            j += dj;
            ++segmentPassed;
            yield return new Vector2i(i, j);

            if (segmentPassed != segmentLength)
                continue;

            segmentPassed = 0;

            int buffer = di;
            di = -dj;
            dj = buffer;

            if (dj == 0)
                ++segmentLength;
        }
    }
}