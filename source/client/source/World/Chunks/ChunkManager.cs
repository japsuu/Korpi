using System.Diagnostics;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Mathematics;
using Korpi.Client.Meshing;
using Korpi.Client.Physics;
using Korpi.Client.Registries;
using Korpi.Client.Rendering;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

/// <summary>
/// Manages all loaded <see cref="ChunkColumn"/>s.
/// </summary>
public class ChunkManager
{
    private const int MAX_CHUNKS_TO_LOAD_PER_TICK = 8;
    private const int MAX_CHUNKS_TO_UNLOAD_PER_TICK = 8;
    
    private static readonly Logging.IKorpiLogger Logger = Logging.LogFactory.GetLogger(typeof(ChunkManager));
    
    private readonly Dictionary<Vector2i, ChunkColumn> _loadedColumns = new();
    private readonly SortedSet<ChunkColumn> _loadedSortedColumns = new(new ChunkDistanceComparer()); //NOTE: This doesn't keep the chunks sorted when the playerEntity moves.
    private readonly Queue<Vector2i> _columnLoadQueue = new();
    private readonly Queue<Vector2i> _columnUnloadQueue = new();
    private readonly HashSet<Vector2i> _columnsToLoad = new();
    private readonly HashSet<Vector2i> _columnsToUnload = new();

    /// <summary>
    /// Precomputed spiral of chunk column positions to load.
    /// The spiral is centered around the world origin, so manual offsetting is required.
    /// </summary>
    private List<Vector2i> _columnLoadSpiral = null!;

    public int LoadedColumnsCount => _loadedColumns.Count;


    public ChunkManager()
    {
        PrecomputeChunkLoadSpiral();
    }


    public void Tick()
    {
        FindChunksToUnload(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        UnloadChunks();
        FindChunksToLoad(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        LoadChunks();

        // Tick all loaded columns
        foreach (ChunkColumn chunk in _loadedSortedColumns)
        {
            chunk.Tick();
        }
    }


    public void DrawChunks(RenderPass pass)
    {
        foreach (ChunkColumn chunk in _loadedSortedColumns)
        {
            chunk.Draw(pass);
        }
    }


#if DEBUG
    public static void DrawDebugBorders()
    {
        if (ClientConfig.DebugModeConfig.RenderChunkBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector3i chunkPos = CoordinateUtils.WorldToChunk(Rendering.Cameras.Camera.RenderingCamera.Position);
            Debugging.Drawing.DebugChunkDrawer.DrawChunkBorders(chunkPos);
        }

        if (ClientConfig.DebugModeConfig.RenderColumnBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector2i columnPos = CoordinateUtils.WorldToColumn(Rendering.Cameras.Camera.RenderingCamera.Position);
            Debugging.Drawing.DebugChunkDrawer.DrawChunkColumnBorders(columnPos);
        }
    }
#endif


    /// <summary>
    /// Checks if a chunk exists at the given position.
    /// The chunk is not guaranteed to be loaded.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns true if a chunk exists at the given position, false otherwise</returns>
    public bool ChunkExistsAt(Vector3i chunkPos)
    {
        Vector2i columnPos = new(chunkPos.X, chunkPos.Z);
        
        return _loadedColumns.ContainsKey(columnPos);
    }

    
    /// <summary>
    /// Gets the chunk at the given position.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns the chunk at the given position, or null if the chunk is not loaded</returns>
    public Chunk? GetChunkAt(Vector3i position)
    {
        Vector2i chunkColumnPos = CoordinateUtils.WorldToColumn(position);

        if (!_loadedColumns.TryGetValue(chunkColumnPos, out ChunkColumn? column))
            return null;
        
        Debug.Assert(position.Y is >= 0 and < Constants.CHUNK_COLUMN_HEIGHT_BLOCKS, $"Tried to get chunk at {position}, but the Y coordinate was out of range!");

        return column.GetChunkAtHeight(position.Y);
    }

    
    /// <summary>
    /// Gets the chunk at the given position.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns the chunk at the given position, or null if the chunk is not loaded</returns>
    public ChunkColumn? GetChunkAt(Vector2i chunkPos)
    {
        Debug.Assert(chunkPos.X % Constants.CHUNK_SIDE_LENGTH == 0, $"Tried to get chunk at {chunkPos}, but the X coordinate was not a multiple of {Constants.CHUNK_SIDE_LENGTH}!");
        Debug.Assert(chunkPos.Y % Constants.CHUNK_SIDE_LENGTH == 0, $"Tried to get chunk at {chunkPos}, but the Y coordinate was not a multiple of {Constants.CHUNK_SIDE_LENGTH}!");

        _loadedColumns.TryGetValue(chunkPos, out ChunkColumn? chunk);
        return chunk;
    }


    /// <summary>
    /// Fills the given array with data of the chunk at the given position.
    /// The array also contains 1 block wide slices of the neighbouring chunks.
    /// Thread safe.
    /// </summary>
    /// <param name="chunkOriginPos">Position of the center chunk</param>
    /// <param name="cache">Array to fill with BlockState data</param>
    public void FillMeshingCache(Vector3i chunkOriginPos, MeshingDataCache cache)
    {
        Chunk? loadedChunk = GetChunkAt(chunkOriginPos);

        if (loadedChunk == null)
            throw new InvalidOperationException($"Tried to fill meshing cache at {chunkOriginPos}, but the chunk was not loaded!");

        cache.Clear();

        // Copy the block data of the center chunk
        cache.SetCenterChunk(loadedChunk);
    }


    /// <summary>
    /// Inefficiently gets the block state at the given world position.
    /// Do not use this method for getting multiple blocks at once.
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public BlockState GetBlockStateAtWorld(Vector3i worldPosition)
    {
        if (worldPosition.Y < 0 || worldPosition.Y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            return BlockRegistry.Air.GetDefaultState();
        
        Chunk? chunk = GetChunkAt(worldPosition);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateUtils.WorldToChunkRelative(worldPosition);
        return chunk.GetBlockState(new ChunkBlockPosition(chunkRelativePos));
    }
    
    
    /// <summary>
    /// Inefficiently sets the block state at the given world position.
    /// Do not use this method for setting multiple blocks at once.
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <param name="blockState"></param>
    /// <returns></returns>
    public BlockState SetBlockStateAtWorld(Vector3i worldPosition, BlockState blockState)
    {
        if (worldPosition.Y < 0 || worldPosition.Y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            return BlockRegistry.Air.GetDefaultState();
        
        Chunk? chunk = GetChunkAt(worldPosition);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateUtils.WorldToChunkRelative(worldPosition);
        chunk.SetBlockState(new ChunkBlockPosition(chunkRelativePos), blockState, out BlockState oldBlockState, false);

        return oldBlockState;
    }


    private void FindChunksToUnload(Vector3 playerPos)
    {
        Vector2i originColumnPos = CoordinateUtils.WorldToColumn(playerPos);
        foreach ((Vector2i position, ChunkColumn? chunk) in _loadedColumns)
        {
            Vector2i normalizedColumnPos = (position - originColumnPos) / Constants.CHUNK_SIDE_LENGTH;
            if (Constants.CIRCULAR_LOAD_REGION)
            {
                bool inRange = normalizedColumnPos.X * normalizedColumnPos.X + normalizedColumnPos.Y * normalizedColumnPos.Y <=
                               Constants.CHUNK_UNLOAD_RADIUS * Constants.CHUNK_UNLOAD_RADIUS;
                if (inRange)
                    continue;
            }
            else
            {
                bool inRange = Math.Abs(normalizedColumnPos.X) <= Constants.CHUNK_UNLOAD_RADIUS &&
                               Math.Abs(normalizedColumnPos.Y) <= Constants.CHUNK_UNLOAD_RADIUS;
                if (inRange)
                    continue;
            }

            if (chunk.ReadyToUnload() && !_columnsToUnload.Contains(position))
            {
                _columnUnloadQueue.Enqueue(position);
                _columnsToUnload.Add(position);
            }
        }
    }


    private void UnloadChunks()
    {
        int i = 0;
        while (i < MAX_CHUNKS_TO_UNLOAD_PER_TICK && _columnUnloadQueue.TryDequeue(out Vector2i chunkPos))
        {
            if (!_loadedColumns.Remove(chunkPos, out ChunkColumn? chunk))
                continue;
            
            chunk.Unload();
            _loadedSortedColumns.Remove(chunk);
            _columnsToUnload.Remove(chunkPos);
            i++;
        }
    }


    private void FindChunksToLoad(Vector3 playerPos)
    {
        // Get the column position where the loading should start
        Vector2i originColumnPos = CoordinateUtils.WorldToColumn(playerPos);

        // Load columns in a square around the origin column in a spiral pattern.
        foreach (Vector2i spiralPos in _columnLoadSpiral)
        {
            Vector2i columnPos = originColumnPos + spiralPos;
            if (_loadedColumns.ContainsKey(columnPos) || _columnsToLoad.Contains(columnPos))
                continue;

            _columnLoadQueue.Enqueue(columnPos);
            _columnsToLoad.Add(columnPos);
        }
    }


    private void LoadChunks()
    {
        int i = 0;
        while (i < MAX_CHUNKS_TO_LOAD_PER_TICK && _columnLoadQueue.TryDequeue(out Vector2i chunkPos))
        {
            ChunkColumn chunkColumn = new(chunkPos);
            chunkColumn.Load();
            if (!_loadedColumns.TryAdd(chunkPos, chunkColumn))
                Logger.Error($"Failed to add chunk column at {chunkPos} to loaded columns!");
            _loadedSortedColumns.Add(chunkColumn);
            _columnsToLoad.Remove(chunkPos);
            i++;
        }
    }


    private void PrecomputeChunkLoadSpiral()
    {
        const int size = Constants.CHUNK_LOAD_RADIUS * 2 + 1;
        _columnLoadSpiral = new List<Vector2i>
        {
            new(0, 0)
        };

        foreach (Vector2i pos in EnumerateSpiral(size * size - 1))
        {
            if (Constants.CIRCULAR_LOAD_REGION)
            {
                bool inRange = pos.X * pos.X + pos.Y * pos.Y <= Constants.CHUNK_LOAD_RADIUS * Constants.CHUNK_LOAD_RADIUS;
                
                // Ensure that the position is inside the load radius
                if (!inRange)
                    continue;
            }
            _columnLoadSpiral.Add(pos * Constants.CHUNK_SIDE_LENGTH);
        }

        Logger.Info($"Precomputed column load spiral for render distance {Constants.CHUNK_LOAD_RADIUS}, for {_columnLoadSpiral.Count} columns.");
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
                Debugging.Drawing.DebugDrawer.DrawBox(new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f), Vector3.One, Color4.Red);
#endif

            BlockState blockState = GetBlockStateAtWorld(blockPos);
            
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
    
    
    private class ChunkDistanceComparer : IComparer<ChunkColumn>
    {
        public int Compare(ChunkColumn x, ChunkColumn y)
        {
            Vector3 playerPosition = PlayerEntity.LocalPlayerEntity.Transform.LocalPosition;
            float xDistance = (x.Position - playerPosition.Xz).LengthSquared;
            float yDistance = (y.Position - playerPosition.Xz).LengthSquared;

            int distanceComparison = xDistance.CompareTo(yDistance);
            if (distanceComparison != 0)
            {
                return distanceComparison;
            }

            // If the distances are equal, compare the chunks' positions
            int xComparison = x.Position.X.CompareTo(y.Position.X);
            if (xComparison != 0)
            {
                return xComparison;
            }

            return x.Position.Y.CompareTo(y.Position.Y);
        }
    }
}