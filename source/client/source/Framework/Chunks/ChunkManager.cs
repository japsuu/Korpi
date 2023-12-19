using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Configuration;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Meshing;
using BlockEngine.Client.Framework.Physics;
using BlockEngine.Client.Framework.Registries;
using BlockEngine.Client.Framework.Rendering.Shaders;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkManager
{
    /// <summary>
    /// Contains all 26 neighbouring chunk offsets.
    /// </summary>
    private static readonly Vector3i[] NeighbouringChunkOffsets =
    {
        // 8 Corners
        new(1, 1, 1),
        new(-1, 1, 1),
        new(-1, 1, -1),
        new(1, 1, -1),
        new(1, -1, 1),
        new(-1, -1, 1),
        new(-1, -1, -1),
        new(1, -1, -1),

        // 12 Edges
        new(1, 1, 0),
        new(0, 1, 1),
        new(-1, 1, 0),
        new(0, 1, -1),
        new(1, -1, 0),
        new(0, -1, 1),
        new(-1, -1, 0),
        new(0, -1, -1),
        new(1, 0, 1),
        new(-1, 0, 1),
        new(-1, 0, -1),
        new(1, 0, -1),

        // 6 Faces
        new(1, 0, 0),
        new(0, 0, 1),
        new(-1, 0, 0),
        new(0, 0, -1),
        new(0, 1, 0),
        new(0, -1, 0)
    };

    private static readonly NeighbouringChunkPosition[] NeighbouringChunkPositions =
        Enum.GetValues(typeof(NeighbouringChunkPosition)).Cast<NeighbouringChunkPosition>().ToArray();

    private readonly Vector3i[] _precomputedNeighbouringChunkOffsets = new Vector3i[26];
    private readonly Dictionary<Vector2i, ChunkColumn> _loadedColumns = new();
    private readonly List<Vector2i> _columnsToLoad = new();
    private readonly List<Vector2i> _columnsToUnload = new();

    // Precomputed spiral of chunk column positions to load.
    // The spiral is centered around the origin.
    private List<Vector2i> _columnLoadSpiral = null!;
    
    public int LoadedColumnsCount => _loadedColumns.Count;


    public ChunkManager()
    {
        PrecomputeNeighbouringChunkOffsets();
        PrecomputeColumnLoadSpiral();
    }


    public void Tick()
    {
        FindColumnsToUnload(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        UnloadColumns();
        FindColumnsToLoad(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        LoadColumns();

        // Tick all columns
        foreach (ChunkColumn column in _loadedColumns.Values)
        {
            column.Tick();
        }
    }


    public void Draw(Vector3 cameraPos, Shader chunkShader)
    {
        foreach (ChunkColumn column in _loadedColumns.Values)
            for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
            {
                Chunk? chunk = column.GetChunk(i);
                if (chunk?.MeshState != Chunk.ChunkMeshState.Meshed)
                    continue;

                DrawChunkAt(chunk.Position, chunkShader);
            }

#if DEBUG
        if (ClientConfig.DebugModeConfig.RenderChunkBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector3i chunkPos = CoordinateConversions.GetContainingChunkPos(cameraPos);
            DebugChunkDrawer.DrawChunkBorders(chunkPos);
        }

        if (ClientConfig.DebugModeConfig.RenderChunkColumnBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector2i columnPos = CoordinateConversions.GetContainingColumnPos(cameraPos);
            DebugChunkDrawer.DrawChunkColumnBorders(columnPos);
        }
#endif
    }


    public bool IsChunkLoaded(Vector3i chunkPos)
    {
        Vector2i columnPos = CoordinateConversions.GetContainingColumnPos(chunkPos);
        if (!_loadedColumns.TryGetValue(columnPos, out ChunkColumn? column))
            return false;

        return column.GetChunkAtHeight(chunkPos.Y) != null;
    }


    /// <summary>
    /// Fills the given array with data of the chunk at the given position.
    /// The array also contains 1 block wide slices of the neighbouring chunks.
    /// </summary>
    /// <param name="chunkOriginPos">Position of the center chunk</param>
    /// <param name="cache">Array to fill with BlockState data</param>
    /// <returns>If the center chunk has been generated.</returns>
    public Chunk GetChunkAndFillMeshingCache(Vector3i chunkOriginPos, MeshingDataCache cache)
    {
        Chunk? loadedChunk = GetChunkAt(chunkOriginPos);

        if (loadedChunk == null)
            throw new InvalidOperationException($"Tried to fill meshing cache at {chunkOriginPos}, but the chunk was not loaded!");

        Array.Fill(cache.Data, BlockRegistry.Air.GetDefaultState());

        // Copy the block data of the center chunk
        loadedChunk.CacheMeshingData(cache);

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

        return loadedChunk;
    }


    public void ReloadAllChunks()
    {
        foreach (ChunkColumn column in _loadedColumns.Values)
        {
            for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
            {
                Chunk? chunk = column.GetChunk(i);

                chunk?.SetMeshDirty();
            }
        }
    }


    public BlockState GetBlockStateAt(Vector3i position)
    {
        Chunk? chunk = GetChunkAt(position);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateConversions.GetChunkRelativePos(position);
        return chunk.GetBlockState(chunkRelativePos);
    }


    private void FindColumnsToUnload(Vector3 playerPos)
    {
        _columnsToUnload.Clear();
        Vector2i originColumnPos = CoordinateConversions.GetContainingColumnPos(playerPos);
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
                ChunkRendererStorage.RemoveRenderer(chunkPos);
            }

            // TODO: Check if this chunk was queued for meshing, and remove it from the queue.

            // Logger.Log($"Unloaded chunk column at {columnPos}.");
        }

        // if (_columnsToUnload.Count > 0)
        //     Logger.Log($"Unloaded {_columnsToUnload.Count * Constants.CHUNK_COLUMN_HEIGHT} chunks.");
    }


    private void FindColumnsToLoad(Vector3 playerPos)
    {
        _columnsToLoad.Clear();

        // Get the column position where the loading should start
        Vector2i originColumnPos = CoordinateConversions.GetContainingColumnPos(playerPos);

        // Load columns in a square around the origin column in a spiral pattern.
        foreach (Vector2i spiralPos in _columnLoadSpiral)
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
            ChunkColumn column = new(columnPos);
            column.Load();
            _loadedColumns.Add(columnPos, column);

            // Logger.Log($"Loaded chunk column at {columnPos}.");
        }
    }


    public Chunk? GetChunkAt(Vector3i position)
    {
        Vector2i chunkColumnPos = CoordinateConversions.GetContainingColumnPos(position);

        if (!_loadedColumns.TryGetValue(chunkColumnPos, out ChunkColumn? column))

            //Logger.LogWarning($"Tried to get unloaded ChunkColumn at {position} ({chunkColumnPos})!");
            return null;

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
            new(0, 0)
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


    public RaycastResult RaycastBlocks(Ray ray, float maxDistance)
    {
        Vector3 direction = ray.NormalizedDirection;
        Vector3 position = ray.Start;

        return Raycast(position, direction, maxDistance);
    }


    private RaycastResult Raycast(Vector3 startPos, Vector3 direction, float maxDistance)
    {
        Vector3i step = new(Math.Sign(direction.X), Math.Sign(direction.Y), Math.Sign(direction.Z));

        Vector3 directionAbs = new(Math.Abs(direction.X), Math.Abs(direction.Y), Math.Abs(direction.Z));
        Vector3 posOffset = startPos - new Vector3((float)Math.Floor(startPos.X), (float)Math.Floor(startPos.Y), (float)Math.Floor(startPos.Z)) -
                            Vector3.ComponentMax(step, Vector3.Zero);
        Vector3 posOffsetAbs = new(Math.Abs(posOffset.X), Math.Abs(posOffset.Y), Math.Abs(posOffset.Z));
        Vector3 nextIntersectionDistance = posOffsetAbs / directionAbs; // Distance to the next intersection with a block boundary.

        Vector3 intersectionDistanceDelta = Vector3.One / directionAbs; // Change in intersection distance when moving to the next block boundary.

        Vector3i blockPos = new((int)Math.Floor(startPos.X), (int)Math.Floor(startPos.Y), (int)Math.Floor(startPos.Z));

        int itr = 0;
        float travelledDistance = 0;
        while (++itr < 100 && travelledDistance < maxDistance)
        {
            int intersectedFace;    // The face of the block that the ray intersected with.
            Vector3 intersectionDistance = nextIntersectionDistance; // Cache the old tMax value to later calculate the intersection point.
            if (nextIntersectionDistance.X < nextIntersectionDistance.Y && nextIntersectionDistance.X < nextIntersectionDistance.Z)
            {
                blockPos.X += step.X;
                nextIntersectionDistance.X += intersectionDistanceDelta.X;
                intersectedFace = step.X > 0 ? 3 : 0;
            }
            else if (nextIntersectionDistance.Y < nextIntersectionDistance.Z)
            {
                blockPos.Y += step.Y;
                nextIntersectionDistance.Y += intersectionDistanceDelta.Y;
                intersectedFace = step.Y > 0 ? 4 : 1;
            }
            else
            {
                blockPos.Z += step.Z;
                nextIntersectionDistance.Z += intersectionDistanceDelta.Z;
                intersectedFace = step.Z > 0 ? 5 : 2;
            }

#if DEBUG
            if (ClientConfig.DebugModeConfig.RenderRaycastPath)
                DebugDrawer.DrawBox(new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f), Vector3.One, Color4.Red);
#endif

            BlockState blockState = GetBlockStateAt(blockPos);
            
            // Calculate the intersection point (travelled distance).
            travelledDistance = Math.Min(Math.Min(intersectionDistance.X, intersectionDistance.Y), intersectionDistance.Z);

            if (blockState.IsAir)
                continue;
            
            Vector3 hitPos = startPos + direction * travelledDistance;
            return new RaycastResult(true, hitPos, blockPos, (BlockFace)intersectedFace, blockState);
        }

        Vector3 rayEnd = startPos + direction * maxDistance;
        return new RaycastResult(false, rayEnd, blockPos, 0, BlockRegistry.Air.GetDefaultState());
    }
}