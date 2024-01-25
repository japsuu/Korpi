using System.Collections.Concurrent;
using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Logging;
using Korpi.Client.Mathematics;
using Korpi.Client.Meshing;
using Korpi.Client.Physics;
using Korpi.Client.Registries;
using Korpi.Client.Rendering;
using Korpi.Client.World.Chunks;
using Korpi.Client.World.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.World;

/// <summary>
/// Manages all loaded <see cref="SubChunk"/>s and chunk columns (<see cref="Chunk"/>s).
/// </summary>
public class RegionManager
{
    private readonly ConcurrentDictionary<Vector2i, Chunk> _existingRegions = new();
    private readonly List<Vector2i> _regionsToLoad = new();
    private readonly List<Vector2i> _chunksToUnload = new();

    /// <summary>
    /// Precomputed spiral of chunk column positions to load.
    /// The spiral is centered around the world origin, so manual offsetting is required.
    /// </summary>
    private List<Vector2i> _regionLoadSpiral = null!;

    public int LoadedChunksCount => _existingRegions.Count;


    public RegionManager()
    {
        PrecomputeRegionLoadSpiral();
    }


    public void Tick()
    {
        FindRegionsToUnload(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        UnloadChunks();
        FindRegionsToLoad(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        LoadRegions();

        // Tick all loaded columns
        foreach (Chunk region in _existingRegions.Values)
        {
            region.Tick();
        }
    }


    public void DrawChunks(RenderPass pass)
    {
        foreach (Chunk chunk in _existingRegions.Values)      // TODO: Instead of doing this, loop the renderer storage and draw all those meshes
        {
            chunk.Draw(pass);                     //TODO: Draw chunks in order of distance to player, to reduce overdraw
        }
    }


#if DEBUG
    public static void DrawDebugBorders()
    {
        if (ClientConfig.DebugModeConfig.RenderChunkBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector3i chunkPos = CoordinateUtils.WorldToSubChunk(Camera.RenderingCamera.Position);
            DebugChunkDrawer.DrawChunkBorders(chunkPos);
        }

        if (ClientConfig.DebugModeConfig.RenderRegionBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector2i columnPos = CoordinateUtils.WorldToColumn(Camera.RenderingCamera.Position);
            DebugChunkDrawer.DrawChunkColumnBorders(columnPos);
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
        Vector2i columnPos = new Vector2i(chunkPos.X, chunkPos.Z);
        
        return _existingRegions.TryGetValue(columnPos, out Chunk? column);
    }

    
    /// <summary>
    /// Gets the subchunk at the given position.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns the chunk at the given position, or null if the chunk is not loaded</returns>
    public SubChunk? GetSubChunkAt(Vector3i position)
    {
        Vector2i chunkColumnPos = CoordinateUtils.WorldToColumn(position);

        if (!_existingRegions.TryGetValue(chunkColumnPos, out Chunk? column))
            return null;
        
        Debug.Assert(position.Y is >= 0 and < Constants.CHUNK_HEIGHT_BLOCKS, $"Tried to get chunk at {position}, but the Y coordinate was out of range!");

        return column.GetSubchunkAtHeight(position.Y);
    }

    
    /// <summary>
    /// Gets the chunk at the given position.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns the chunk at the given position, or null if the chunk is not loaded</returns>
    public Chunk? GetChunkAt(Vector2i chunkPos)
    {
        Debug.Assert(chunkPos.X % Constants.SUBCHUNK_SIDE_LENGTH == 0, $"Tried to get chunk at {chunkPos}, but the X coordinate was not a multiple of {Constants.SUBCHUNK_SIDE_LENGTH}!");
        Debug.Assert(chunkPos.Y % Constants.SUBCHUNK_SIDE_LENGTH == 0, $"Tried to get chunk at {chunkPos}, but the Y coordinate was not a multiple of {Constants.SUBCHUNK_SIDE_LENGTH}!");

        _existingRegions.TryGetValue(chunkPos, out Chunk? chunk);
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
        SubChunk? loadedChunk = GetSubChunkAt(chunkOriginPos);

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
        if (worldPosition.Y < 0 || worldPosition.Y >= Constants.CHUNK_HEIGHT_BLOCKS)
            return BlockRegistry.Air.GetDefaultState();
        
        SubChunk? chunk = GetSubChunkAt(worldPosition);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateUtils.WorldToChunkRelative(worldPosition);
        return chunk.GetBlockState(new SubChunkBlockPosition(chunkRelativePos));
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
        if (worldPosition.Y < 0 || worldPosition.Y >= Constants.CHUNK_HEIGHT_BLOCKS)
            return BlockRegistry.Air.GetDefaultState();
        
        SubChunk? chunk = GetSubChunkAt(worldPosition);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateUtils.WorldToChunkRelative(worldPosition);
        chunk.SetBlockState(new SubChunkBlockPosition(chunkRelativePos), blockState, out BlockState oldBlockState, false);

        return oldBlockState;
    }


    private void FindRegionsToUnload(Vector3 playerPos)
    {
        _chunksToUnload.Clear();
        Vector2i originColumnPos = CoordinateUtils.WorldToColumn(playerPos);
        foreach (KeyValuePair<Vector2i, Chunk> pair in _existingRegions)
        {
            Vector2i normalizedColumnPos = (pair.Key - originColumnPos) / Constants.SUBCHUNK_SIDE_LENGTH;
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
            Chunk chunk = pair.Value;
            if (chunk.ReadyToUnload())
                _chunksToUnload.Add(pair.Key);
        }
    }


    private void UnloadChunks()
    {
        foreach (Vector2i columnPos in _chunksToUnload)
        {
            if (!_existingRegions.TryRemove(columnPos, out Chunk? column))
                continue;
            
            column.Unload();
        }
    }


    private void FindRegionsToLoad(Vector3 playerPos)
    {
        _regionsToLoad.Clear();

        // Get the column position where the loading should start
        Vector2i originColumnPos = CoordinateUtils.WorldToColumn(playerPos);

        // Load columns in a square around the origin column in a spiral pattern.
        foreach (Vector2i spiralPos in _regionLoadSpiral)
        {
            Vector2i columnPos = originColumnPos + spiralPos;
            if (_existingRegions.ContainsKey(columnPos))
                continue;

            _regionsToLoad.Add(columnPos);
        }
    }


    private void LoadRegions()
    {
        foreach (Vector2i columnPos in _regionsToLoad)
        {
            Chunk column = new(columnPos);
            column.Load();
            if (!_existingRegions.TryAdd(columnPos, column))
                Logger.LogError($"Failed to add chunk column at {columnPos} to loaded columns!");
        }
    }


    private void PrecomputeRegionLoadSpiral()
    {
        const int size = Constants.CHUNK_LOAD_RADIUS * 2 + 1;
        _regionLoadSpiral = new List<Vector2i>
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
            _regionLoadSpiral.Add(pos * Constants.SUBCHUNK_SIDE_LENGTH);
        }

        Logger.Log($"Precomputed column load spiral for render distance {Constants.CHUNK_LOAD_RADIUS}, for {_regionLoadSpiral.Count} columns.");
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
}