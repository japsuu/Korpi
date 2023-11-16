using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Chunks;

public class ChunkManager
{
    private readonly Dictionary<Vector2i, ChunkColumn> _loadedColumns = new();
    private readonly List<Vector2i> _columnsToLoad = new();
    private readonly List<Vector2i> _columnsToUnload = new();

    private readonly World _world;

    // Precomputed spiral of chunk column positions to load.
    // The spiral is centered around the origin.
    // TODO: Convert to hashset for faster lookup?
    private List<Vector2i> _chunkLoadSpiral = null!;


    public ChunkManager(World world)
    {
        _world = world;
        PrecalculateChunkLoadSpiral();
    }


    public BlockState GetBlockAt(Vector3i position)
    {
        Chunk? chunk = GetChunkAt(position);
        if (chunk == null)
            return Blocks.Blocks.Air.GetDefaultState();

        Vector3i chunkRelativePosition = CoordinateConversions.GetChunkRelativePos(position);
        return chunk.GetBlockState(chunkRelativePosition);
    }


    public void Tick(Vector3 cameraPos, double deltaTime)
    {
        FindColumnsToUnload(cameraPos);
        UnloadColumns();
        FindColumnsToLoad(cameraPos);
        LoadColumns();
        
        // Tick all columns
        foreach (ChunkColumn column in _loadedColumns.Values)
            column.Tick(deltaTime);
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
            _loadedColumns[columnPos].Unload();
            _loadedColumns.Remove(columnPos);

            // Logger.Log($"Unloaded chunk column at {columnPos}.");
        }

        if (_columnsToUnload.Count > 0)
            Logger.Log($"Unloaded {_columnsToUnload.Count} chunks.");
    }


    private void FindColumnsToLoad(Vector3 cameraPos)
    {
        _columnsToLoad.Clear();

        // Get the column position where the loading should start
        Vector2i originColumnPos = CoordinateConversions.GetContainingColumnPos(cameraPos);

        // Load columns in a square around the origin column in a spiral pattern.
        foreach (Vector2i spiralPos in _chunkLoadSpiral)
        {
            Vector2i columnPos = originColumnPos + spiralPos;
            if (_loadedColumns.ContainsKey(columnPos))
                continue;

            _columnsToLoad.Add(columnPos);
        }
    }


    private void LoadColumns()
    {
        foreach (Vector2i columnPos in _columnsToLoad)
        {
            ChunkColumn column = new(_world);
            column.Load();
            _loadedColumns.Add(columnPos, column);

            // Logger.Log($"Loaded chunk column at {columnPos}.");
        }

        if (_columnsToLoad.Count > 0)
            Logger.Log($"Loaded {_columnsToLoad.Count} chunks.");
    }


    private Chunk? GetChunkAt(Vector3i position)
    {
        Vector2i chunkColumnPos = CoordinateConversions.GetContainingColumnPos(position);

        if (!_loadedColumns.TryGetValue(chunkColumnPos, out ChunkColumn? column))
        {
            Logger.LogWarning($"Tried to get unloaded ChunkColumn at {position} ({chunkColumnPos})!");
            return null;
        }

        return column.GetChunkAtHeight(position.Y);
    }


    private void PrecalculateChunkLoadSpiral()
    {
        const int size = Constants.CHUNK_COLUMN_LOAD_RADIUS * 2 + 1;
        _chunkLoadSpiral = new List<Vector2i>
        {
            new Vector2i(0, 0)
        };

        foreach (Vector2i pos in EnumerateSpiral(size * size - 1))
        {
            bool inRange = pos.X * pos.X + pos.Y * pos.Y <= Constants.CHUNK_COLUMN_LOAD_RADIUS_SQUARED;

            // Ensure that the position is inside the load radius
            if (!inRange)
                continue;
            _chunkLoadSpiral.Add(pos * Constants.CHUNK_SIZE);
        }

        Logger.Log($"Precomputed chunk load spiral for render distance {Constants.CHUNK_COLUMN_LOAD_RADIUS}, for {_chunkLoadSpiral.Count} chunks.");
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