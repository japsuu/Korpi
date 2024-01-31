using System.Diagnostics;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Generation.Jobs;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Threading.Pooling;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

/// <summary>
/// Vertical column of chunks.
/// </summary>
public class ChunkColumn : IChunkColumn
{
    private static readonly Logging.IKorpiLogger Logger = Logging.LogFactory.GetLogger(typeof(ChunkColumn));
    
    private ChunkGenerationState _currentState = ChunkGenerationState.UNINITIALIZED;
    private long _currentJobId;
    private readonly Chunk[] _chunks;
    private readonly ChunkHeightmap _heightmap;

    public Vector2i Position { get; }
    
    /// <summary>
    /// Lock used to synchronize access to this chunk.
    /// </summary>
    public readonly ReaderWriterLockSlim ThreadLock;
    
    /// <summary>
    /// Id of the job last executed on this chunk.
    /// </summary>
    public long CurrentJobId => Interlocked.Read(ref _currentJobId);
    
    private static Vector2i DebugPosition = new(96, 96);
    private static Stopwatch DebugSw = new();
    private static double PreviousMillis = 0;
        
        
    public ChunkColumn(Vector2i position)
    {
        Position = position;
        _chunks = new Chunk[Constants.CHUNK_COLUMN_HEIGHT_CHUNKS];
        _heightmap = new ChunkHeightmap();
        ThreadLock = new ReaderWriterLockSlim();
        
        GameWorld.WorldEventPublished += WorldEventHandler;
    }
        
    
    /// <returns>If this chunk can be safely unloaded.</returns>
    public bool ReadyToUnload() => true;
    
    
    /// <returns>The highest block at the given x and z coordinates. -1 if no blocks are found.</returns>
    public int GetHighestBlock(int x, int z) => _heightmap.GetHighestBlock(x, z);
        
    
    /// <returns>The chunk at the given height.</returns>
    public Chunk GetChunkAtHeight(int y) => _chunks[y / Constants.CHUNK_SIDE_LENGTH];


    /// <summary>
    /// Loads this column, and creates and loads all contained chunks.
    /// </summary>
    public void Load()
    {
        for (int i = Constants.CHUNK_COLUMN_HEIGHT_CHUNKS - 1; i >= 0; i--)
        {
            Chunk chunk = new(this, i * Constants.CHUNK_SIDE_LENGTH);
            chunk.Load();
            _chunks[i] = chunk;
        }
        if (Position == DebugPosition)
        {
            DebugSw.Start();
            Logger.Debug($"Column {Position} loaded");
        }

        ChangeState(ChunkGenerationState.GENERATING_TERRAIN);
    }


    /// <summary>
    /// Ticks this column, and all contained chunks.
    /// </summary>
    public void Tick()
    {
        ExecuteCurrentState();
        
        foreach (Chunk chunk in _chunks)
            chunk.Tick();
    }
    
    
    /// <summary>
    /// Draws this column, and all contained chunks.
    /// </summary>
    /// <param name="pass">The render pass to draw with.</param>
    public void Draw(RenderPass pass)
    {
        // Frustum check.
#if DEBUG

        // If in debug mode, allow the player to toggle frustum culling on/off
        if (ClientConfig.DebugModeConfig.DoFrustumCulling)
        {
            Frustum cameraViewFrustum = ClientConfig.DebugModeConfig.OnlyPlayerFrustumCulling
                ? PlayerEntity.LocalPlayerEntity.Camera.ViewFrustum
                : Camera.RenderingCamera.ViewFrustum;

            if (!IsOnFrustum(cameraViewFrustum))
                return;
        }
#else
        if (!IsOnFrustum(PlayerEntity.LocalPlayerEntity.Camera.ViewFrustum))
            return;
#endif
        
        //TODO: Render the chunk the player is in, first.
        for (int i = _chunks.Length - 1; i >= 0; i--)
        {
            Chunk chunk = _chunks[i];
            chunk.Draw(pass);
        }
    }
        
    
    /// <summary>
    /// Unloads this column, and all contained chunks.
    /// </summary>
    public void Unload()
    {
        foreach (Chunk chunk in _chunks)
            chunk.Unload();
        ChangeState(ChunkGenerationState.UNINITIALIZED);
    }


    /// <summary>
    /// Sets the block state at the given coordinates, and updates the heightmap.
    /// </summary>
    /// <param name="x">X coordinate relative to the chunk.</param>
    /// <param name="y">Y coordinate relative to the chunk.</param>
    /// <param name="z">Z coordinate relative to the chunk.</param>
    /// <param name="block">The new block state.</param>
    /// <param name="oldBlock">The old block state.</param>
    /// <param name="delayedMeshDirtying">Whether or not to delay dirtying the mesh.</param>
    public void SetBlockState(int x, int y, int z, BlockState block, out BlockState oldBlock, bool delayedMeshDirtying)
    {
        Debug.Assert(x >= 0 && x < Constants.CHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_HEIGHT_BLOCKS);
        Debug.Assert(z >= 0 && z < Constants.CHUNK_SIDE_LENGTH);
        
        int arrayIndex = y / Constants.CHUNK_SIDE_LENGTH;
        ChunkBlockPosition pos = new(x, y % Constants.CHUNK_SIDE_LENGTH, z);
        _chunks[arrayIndex].SetBlockState(pos, block, out oldBlock, delayedMeshDirtying);
        
        // Update heightmap
        if (block.IsAir)
            _heightmap.OnBlockRemoved(x, y, z);
        else
            _heightmap.OnBlockAdded(x, y, z);
    }


    /// <summary>
    /// Gets the block state at the given coordinates.
    /// </summary>
    /// <param name="x">X coordinate relative to the chunk.</param>
    /// <param name="y">Y coordinate relative to the chunk.</param>
    /// <param name="z">Z coordinate relative to the chunk.</param>
    /// <returns>The block state at the given coordinates.</returns>
    public BlockState GetBlockState(int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Constants.CHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_HEIGHT_BLOCKS);
        Debug.Assert(z >= 0 && z < Constants.CHUNK_SIDE_LENGTH);
        
        int arrayIndex = y / Constants.CHUNK_SIDE_LENGTH;
        ChunkBlockPosition pos = new(x, y % Constants.CHUNK_SIDE_LENGTH, z);
        return _chunks[arrayIndex].GetBlockState(pos);
    }
    
    
    public bool IsOnFrustum(Frustum viewFrustum)
    {
        Vector3 position = new Vector3(Position.X, 0, Position.Y);
        Vector3 min = position;
        Vector3 max = position + new Vector3(Constants.CHUNK_SIDE_LENGTH, Constants.CHUNK_HEIGHT_BLOCKS, Constants.CHUNK_SIDE_LENGTH);

        foreach (FrustumPlane plane in viewFrustum.Planes)
        {
            Vector3 pVertex = min;

            if (plane.Normal.X >= 0)
                pVertex.X = max.X;
            if (plane.Normal.Y >= 0)
                pVertex.Y = max.Y;
            if (plane.Normal.Z >= 0)
                pVertex.Z = max.Z;

            if (Vector3.Dot(plane.Normal, pVertex) + plane.Distance < 0)
                return false;
        }

        return true;
    }


    private void ChangeState(ChunkGenerationState newState)
    {
        if (_currentState == newState)
        {
            Logger.Warn($"ChunkColumn {Position} tried to change to state {newState} but is already in that state.");
            return;
        }
        ChunkGenerationState previousState = _currentState;
        OnExitState(previousState);
        _currentState = newState;
        OnEnterState(newState);
    }


    private void OnEnterState(ChunkGenerationState newState)
    {
        if (Position == DebugPosition)
        {
            double millis = DebugSw.Elapsed.TotalMilliseconds;
            Logger.Debug($"Column {Position} enter state {newState} in {millis} ms (delta: {millis - PreviousMillis} ms)");
            PreviousMillis = millis;
        }
        switch (newState)
        {
            case ChunkGenerationState.UNINITIALIZED:
                break;
            case ChunkGenerationState.GENERATING_TERRAIN:
                const WorkItemPriority priority = WorkItemPriority.Normal;
                // if (IsOnFrustum(PlayerEntity.LocalPlayerEntity.Camera.ViewFrustum))
                //     priority = WorkItemPriority.High;
                Interlocked.Increment(ref _currentJobId);
                GlobalThreadPool.DispatchJob(new GenerationJob(_currentJobId, this, () => ChangeState(ChunkGenerationState.GENERATING_DECORATION)), priority);
                break;
            case ChunkGenerationState.GENERATING_DECORATION:
                ChangeState(ChunkGenerationState.GENERATING_LIGHTING);
                break;
            case ChunkGenerationState.GENERATING_LIGHTING:
                ChangeState(ChunkGenerationState.READY);
                break;
            case ChunkGenerationState.READY:
                for (int i = _chunks.Length - 1; i >= 0; i--)
                {
                    _chunks[i].HasBeenGenerated = true;
                    _chunks[i].SetMeshDirty();
                }
                if (Position == DebugPosition)
                {
                    DebugSw.Stop();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }


    private void ExecuteCurrentState()
    {
        switch (_currentState)
        {
            case ChunkGenerationState.UNINITIALIZED:
                break;
            case ChunkGenerationState.GENERATING_TERRAIN:
                break;
            case ChunkGenerationState.GENERATING_DECORATION:
                break;
            case ChunkGenerationState.GENERATING_LIGHTING:
                break;
            case ChunkGenerationState.READY:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void OnExitState(ChunkGenerationState previousState)
    {
        switch (previousState)
        {
            case ChunkGenerationState.UNINITIALIZED:
                break;
            case ChunkGenerationState.GENERATING_TERRAIN:
                break;
            case ChunkGenerationState.GENERATING_DECORATION:
                break;
            case ChunkGenerationState.GENERATING_LIGHTING:
                break;
            case ChunkGenerationState.READY:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(previousState), previousState, null);
        }
    }


    private void WorldEventHandler(WorldEvent worldEvent)
    {
        switch (worldEvent)
        {
            case WorldEvent.RELOAD_ALL_CHUNKS:
                for (int i = _chunks.Length - 1; i >= 0; i--)
                {
                    Chunk chunk = _chunks[i];
                    chunk.SetMeshDirty();
                }

                break;
            case WorldEvent.LOAD_REGION_CHANGED:
                break;
            case WorldEvent.REGENERATE_ALL_CHUNKS:
                for (int i = _chunks.Length - 1; i >= 0; i--)
                {
                    Chunk chunk = _chunks[i];
                    chunk.Reset();
                }

                ChangeState(ChunkGenerationState.GENERATING_TERRAIN);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent, null);
        }
    }


    /// <summary>
    /// Checks if all neighbouring chunks of this chunk are generated.
    /// </summary>
    /// <param name="excludeMissingChunks">If true, chunks that are not loaded are excluded from neighbourhood checks</param>
    /// <returns>True if all neighbouring chunks are generated, false otherwise</returns>
    public bool AreAllNeighboursGenerated(bool excludeMissingChunks)
    {
        foreach (Vector2i chunkOffset in ChunkOffsets.ChunkColumnNeighbourOffsets)
        {
            Vector2i neighbourPos = Position + chunkOffset;
            
            ChunkColumn? neighbourChunk = GameWorld.CurrentGameWorld.ChunkManager.GetChunkAt(neighbourPos);

            if (neighbourChunk == null)
            {
                if (!excludeMissingChunks)
                    return false;
            }
            else
            {
                if (neighbourChunk._currentState != ChunkGenerationState.READY)
                    return false;
            }
        }
        
        return true;
    }
}