using System.Collections.Concurrent;
using BlockEngine.Client.Configuration;
using BlockEngine.Client.Debugging.Drawing;
using BlockEngine.Client.ECS.Entities;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Math;
using BlockEngine.Client.Meshing;
using BlockEngine.Client.Physics;
using BlockEngine.Client.Registries;
using BlockEngine.Client.Rendering.Cameras;
using BlockEngine.Client.Rendering.Chunks;
using BlockEngine.Client.Rendering.Shaders;
using BlockEngine.Client.World.Regions;
using BlockEngine.Client.World.Regions.Chunks;
using BlockEngine.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;

namespace BlockEngine.Client.World;

/// <summary>
/// Manages all loaded <see cref="Chunk"/>s and chunk columns (<see cref="Region"/>s).
/// </summary>
public class RegionManager
{
    private readonly ConcurrentDictionary<Vector2i, Region> _existingRegions = new();
    private readonly List<Vector2i> _regionsToLoad = new();
    private readonly List<Vector2i> _regionsToUnload = new();

    /// <summary>
    /// Precomputed spiral of chunk column positions to load.
    /// The spiral is centered around the world origin, so manual offsetting is required.
    /// </summary>
    private List<Vector2i> _regionLoadSpiral = null!;

    public int LoadedRegionsCount => _existingRegions.Count;


    public RegionManager()
    {
        PrecomputeRegionLoadSpiral();
    }


    public void Tick()
    {
        FindRegionsToUnload(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        UnloadRegions();
        FindRegionsToLoad(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition);
        LoadRegions();

        // Tick all loaded columns
        foreach (Region region in _existingRegions.Values)
        {
            region.Tick();
        }
    }


    public void Draw(Shader chunkShader)
    {
        foreach (Region column in _existingRegions.Values) // TODO: Instead of doing this, loop the renderer storage and draw all those meshes
        {
            for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
            {
                Chunk? chunk = column.GetChunk(i);
                if (chunk == null)
                    continue;
                
                if (!chunk.ShouldBeRendered)
                    continue;

                DrawChunkAt(chunk.Position, chunkShader);
            }
        }

#if DEBUG
        if (ClientConfig.DebugModeConfig.RenderChunkBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector3i chunkPos = CoordinateUtils.WorldToChunk(Camera.RenderingCamera.Position);
            DebugChunkDrawer.DrawChunkBorders(chunkPos);
        }

        if (ClientConfig.DebugModeConfig.RenderRegionBorders)
        {
            // Get the chunk the playerEntity is currently in
            Vector2i columnPos = CoordinateUtils.WorldToColumn(Camera.RenderingCamera.Position);
            DebugChunkDrawer.DrawChunkColumnBorders(columnPos);
        }
#endif
    }


    /// <summary>
    /// Checks if a chunk exists at the given position.
    /// The chunk is not guaranteed to be loaded.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns true if a chunk exists at the given position, false otherwise</returns>
    public bool ChunkExistsAt(Vector3i chunkPos)
    {
        Vector2i columnPos = new Vector2i(chunkPos.X, chunkPos.Z);
        
        if (!_existingRegions.TryGetValue(columnPos, out Region? column))
            return false;

        return column.HasChunkAtHeight(chunkPos.Y);
    }

    
    /// <summary>
    /// Gets the chunk at the given position.
    /// Thread safe.
    /// </summary>
    /// <returns>Returns the chunk at the given position, or null if the chunk is not loaded</returns>
    public Chunk? GetChunkAt(Vector3i position)
    {
        Vector2i chunkColumnPos = CoordinateUtils.WorldToColumn(position);

        if (!_existingRegions.TryGetValue(chunkColumnPos, out Region? column))
            //Logger.LogWarning($"Tried to get unloaded Region at {position} ({chunkColumnPos})!");
            return null;

        return column.GetChunkAtHeight(position.Y);
    }


    /// <summary>
    /// Checks if all neighbouring chunks of the chunk at the given position are generated.
    /// </summary>
    /// <param name="chunkPos">Position of the chunk whose neighbours we want to check</param>
    /// <param name="excludeMissingChunks">If true, chunks that are not loaded are excluded from neighbourhood checks</param>
    /// <returns>True if all neighbouring chunks are generated, false otherwise</returns>
    public bool AreChunkNeighboursGenerated(Vector3i chunkPos, bool excludeMissingChunks)
    {
        foreach (Vector3i chunkOffset in ChunkOffsets.ChunkNeighbourOffsets)
        {
            Vector3i neighbourPos = chunkPos + chunkOffset;
            Chunk? neighbourChunk = GetChunkAt(neighbourPos);

            if (neighbourChunk == null)
            {
                if (!excludeMissingChunks)
                    return false;
            }
            else
            {
                if (!neighbourChunk.IsGenerated)
                    return false;
            }
        }
        
        return true;
        
        // Vector2i columnPos = new Vector2i(chunkPos.X, chunkPos.Z);
        // 
        // foreach (Vector2i neighbourOffset in _precomputedNeighbouringRegionOffsets)
        // {
        //     Vector2i neighbourPos = columnPos + neighbourOffset;
        //     
        //     if (!_existingRegions.TryGetValue(neighbourPos, out Region? column))
        //     {
        //         if (excludeMissingChunks)
        //             continue;
        //         
        //         return false;
        //     }
        //
        //     Chunk? chunk = column.GetChunkAtHeight(chunkPos.Y);
        //     if (chunk == null)
        //     {
        //         if (!excludeMissingChunks)
        //             return false;
        //     }
        //     else
        //     {
        //         if (!chunk.IsGenerated)
        //             return false;
        //     }
        //
        //     chunk = column.GetChunkAtHeight(chunkPos.Y + 1);
        //     if (chunk == null)
        //     {
        //         if (!excludeMissingChunks)
        //             return false;
        //     }
        //     else
        //     {
        //         if (!chunk.IsGenerated)
        //             return false;
        //     }
        //
        //     chunk = column.GetChunkAtHeight(chunkPos.Y - 1);
        //     if (chunk == null)
        //     {
        //         if (!excludeMissingChunks)
        //             return false;
        //     }
        //     else
        //     {
        //         if (!chunk.IsGenerated)
        //             return false;
        //     }
        // }
        //     
        // if (!_existingRegions.TryGetValue(columnPos, out Region? centerColumn))
        // {
        //     if (excludeMissingChunks)
        //         return true;
        //         
        //     return false;
        // }
        //
        // Chunk? sisterChunk = centerColumn.GetChunkAtHeight(chunkPos.Y + 1);
        // if (sisterChunk == null)
        // {
        //     if (!excludeMissingChunks)
        //         return false;
        // }
        // else
        // {
        //     if (!sisterChunk.IsGenerated)
        //         return false;
        // }
        //
        // sisterChunk = centerColumn.GetChunkAtHeight(chunkPos.Y - 1);
        // if (sisterChunk == null)
        // {
        //     if (!excludeMissingChunks)
        //         return false;
        // }
        // else
        // {
        //     if (!sisterChunk.IsGenerated)
        //         return false;
        // }
        //
        // return true;
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


    public void RemeshAllColumns()
    {
        foreach (Region column in _existingRegions.Values)
        {
            column.RemeshAllChunks();
        }
    }


    public BlockState GetBlockStateAt(Vector3i position)
    {
        Chunk? chunk = GetChunkAt(position);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateUtils.WorldToChunkRelative(position);
        return chunk.GetBlockState(chunkRelativePos);
    }
    
    
    public BlockState SetBlockStateAt(Vector3i position, BlockState blockState)
    {
        Chunk? chunk = GetChunkAt(position);
        if (chunk == null)
            return BlockRegistry.Air.GetDefaultState();

        Vector3i chunkRelativePos = CoordinateUtils.WorldToChunkRelative(position);
        bool wasSetDirty = chunk.SetBlockState(chunkRelativePos, blockState, out BlockState oldBlockState);

        if (!wasSetDirty)
            return oldBlockState;
        
        if(chunkRelativePos.X == 0)
            GetChunkAt(position + new Vector3i(-1, 0, 0))?.SetMeshDirty();
        else if(chunkRelativePos.X == Constants.CHUNK_SIZE - 1)
            GetChunkAt(position + new Vector3i(1, 0, 0))?.SetMeshDirty();
            
        if(chunkRelativePos.Y == 0)
            GetChunkAt(position + new Vector3i(0, -1, 0))?.SetMeshDirty();
        else if(chunkRelativePos.Y == Constants.CHUNK_SIZE - 1)
            GetChunkAt(position + new Vector3i(0, 1, 0))?.SetMeshDirty();
            
        if(chunkRelativePos.Z == 0)
            GetChunkAt(position + new Vector3i(0, 0, -1))?.SetMeshDirty();
        else if(chunkRelativePos.Z == Constants.CHUNK_SIZE - 1)
            GetChunkAt(position + new Vector3i(0, 0, 1))?.SetMeshDirty();

        return oldBlockState;
    }
    
    
    private void RemeshNeighbouringColumns(Vector2i columnPos)
    {
        foreach (Vector2i neighbourOffset in ChunkOffsets.RegionNeighbourOffsets)
        {
            Vector2i neighbourPos = columnPos + neighbourOffset;
            
            if (!_existingRegions.TryGetValue(neighbourPos, out Region? column))
                continue;

            column.RemeshAllChunks();
        }
    }


    private void FindRegionsToUnload(Vector3 playerPos)
    {
        _regionsToUnload.Clear();
        Vector2i originColumnPos = CoordinateUtils.WorldToColumn(playerPos);
        foreach (KeyValuePair<Vector2i, Region> pair in _existingRegions)
        {
            Vector2i normalizedColumnPos = (pair.Key - originColumnPos) / Constants.CHUNK_SIZE;
            bool inRange = normalizedColumnPos.X * normalizedColumnPos.X + normalizedColumnPos.Y * normalizedColumnPos.Y <=
                           Constants.CHUNK_COLUMN_UNLOAD_RADIUS_SQUARED;
            if (inRange)
                continue;
            Region column = pair.Value;
            if (column.ReadyToUnload())
                _regionsToUnload.Add(pair.Key);
        }
    }


    private void UnloadRegions()
    {
        foreach (Vector2i columnPos in _regionsToUnload)
        {
            if (!_existingRegions.TryRemove(columnPos, out Region? column))
                continue;
            
            column.Unload();
            for (int y = 0; y < Constants.CHUNK_COLUMN_HEIGHT; y++)
            {
                Chunk? chunk = column.GetChunk(y);
                if (chunk == null)
                    continue;

                Vector3i chunkPos = new(column.Position.X, y * Constants.CHUNK_SIZE, column.Position.Y);
                ChunkRendererStorage.RemoveChunkMesh(chunkPos);
            }

            // Logger.Log($"Unloaded chunk column at {columnPos}.");
        }

        // if (_regionsToUnload.Count > 0)
        //     Logger.Log($"Unloaded {_regionsToUnload.Count * Constants.CHUNK_COLUMN_HEIGHT} chunks.");
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
            Region column = new(columnPos);
            column.Load();
            if (!_existingRegions.TryAdd(columnPos, column))
                Logger.LogError($"Failed to add chunk column at {columnPos} to loaded columns!");

            RemeshNeighbouringColumns(columnPos);
        }
    }


    private void DrawChunkAt(Vector3i position, Shader chunkShader)
    {
        if (ChunkRendererStorage.TryGetRenderer(position, out ChunkRenderer? mesh))
            mesh!.Draw(chunkShader);
    }


    private void PrecomputeRegionLoadSpiral()
    {
        const int size = Constants.CHUNK_COLUMN_LOAD_RADIUS * 2 + 1;
        _regionLoadSpiral = new List<Vector2i>
        {
            new(0, 0)
        };

        foreach (Vector2i pos in EnumerateSpiral(size * size - 1))
        {
            if (false)
            {
                bool inRange = pos.X * pos.X + pos.Y * pos.Y <= Constants.CHUNK_COLUMN_LOAD_RADIUS_SQUARED;
                
                // Ensure that the position is inside the load radius
                if (!inRange)
                    continue;
            }
            _regionLoadSpiral.Add(pos * Constants.CHUNK_SIZE);
        }

        Logger.Log($"Precomputed column load spiral for render distance {Constants.CHUNK_COLUMN_LOAD_RADIUS}, for {_regionLoadSpiral.Count} columns.");
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
        Vector3i step = new(System.Math.Sign(direction.X), System.Math.Sign(direction.Y), System.Math.Sign(direction.Z));

        Vector3 directionAbs = new(System.Math.Abs(direction.X), System.Math.Abs(direction.Y), System.Math.Abs(direction.Z));
        Vector3 posOffset = startPos - new Vector3((float)System.Math.Floor(startPos.X), (float)System.Math.Floor(startPos.Y), (float)System.Math.Floor(startPos.Z)) -
                            Vector3.ComponentMax(step, Vector3.Zero);
        Vector3 posOffsetAbs = new(System.Math.Abs(posOffset.X), System.Math.Abs(posOffset.Y), System.Math.Abs(posOffset.Z));
        Vector3 nextIntersectionDistance = posOffsetAbs / directionAbs; // Distance to the next intersection with a block boundary.

        Vector3 intersectionDistanceDelta = Vector3.One / directionAbs; // Change in intersection distance when moving to the next block boundary.

        Vector3i blockPos = new((int)System.Math.Floor(startPos.X), (int)System.Math.Floor(startPos.Y), (int)System.Math.Floor(startPos.Z));

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
            travelledDistance = System.Math.Min(System.Math.Min(intersectionDistance.X, intersectionDistance.Y), intersectionDistance.Z);

            if (blockState.IsAir)
                continue;
            
            Vector3 hitPos = startPos + direction * travelledDistance;
            return new RaycastResult(true, hitPos, blockPos, (BlockFace)intersectedFace, blockState);
        }

        Vector3 rayEnd = startPos + direction * maxDistance;
        return new RaycastResult(false, rayEnd, blockPos, 0, BlockRegistry.Air.GetDefaultState());
    }
}