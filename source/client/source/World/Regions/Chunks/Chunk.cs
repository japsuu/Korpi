using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Generation.Jobs;
using Korpi.Client.Logging;
using Korpi.Client.Meshing.Jobs;
using Korpi.Client.Rendering.Chunks;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World.Regions.Chunks.Blocks;
using Korpi.Client.World.Regions.Chunks.BlockStorage;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Regions.Chunks;

public class Chunk
{
    private readonly IBlockStorage _blockStorage = new BlockPalette();
    
    // State machine
    private ChunkState _currentState;

    private long _currentJobId;
    private bool _containsRenderedBlocks;
    private bool _hasBeenMeshed;
    private ChunkOffsets.NeighbourOffsetFlags _neighboursToMeshDirty;
    private bool HasBeenGenerated => _currentState >= ChunkState.WAITING_FOR_MESHING;
    
    /// <summary>
    /// Position of this chunk in the world.
    /// </summary>
    public readonly Vector3i Position;
    
    /// <summary>
    /// Highest possible Y value in this chunk.
    /// </summary>
    public readonly int Top;
    
    /// <summary>
    /// Lowest possible Y value in this chunk.
    /// </summary>
    public readonly int Bottom;
    
    /// <summary>
    /// Lock used to synchronize access to this chunk.
    /// </summary>
    public readonly ReaderWriterLockSlim ThreadLock;
    
    /// <summary>
    /// Id of the job last executed on this chunk.
    /// </summary>
    public long CurrentJobId => Interlocked.Read(ref _currentJobId);


    public Chunk(Vector3i position)
    {
        Position = position;
        Top = position.Y + Constants.CHUNK_SIDE_LENGTH - 1;
        Bottom = position.Y;
        
        _containsRenderedBlocks = false;
        ThreadLock = new ReaderWriterLockSlim();
        
        GameWorld.WorldEventPublished += WorldEventHandler;
    }


    private void WorldEventHandler(WorldEvent worldEvent)
    {
        switch (worldEvent)
        {
            case WorldEvent.RELOAD_ALL_CHUNKS:
                SetMeshDirty();
                break;
            case WorldEvent.LOAD_REGION_CHANGED:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent, null);
        }
    }


    public void Tick()
    {
        ExecuteCurrentState();
    }


    public void Draw()
    {
        if (!_hasBeenMeshed)
            return;

        if (ChunkRendererStorage.TryGetRenderer(Position, out ChunkRenderer? mesh))
            mesh!.Draw();
        
#if DEBUG
        DebugDraw();
#endif
    }


    public void Load()
    {
        ChangeState(ChunkState.GENERATING_TERRAIN);
    }


    public void Unload()
    {
        ChangeState(ChunkState.UNINITIALIZED);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// <code>
    /// for z in range:
    ///     for y in range:
    ///         for x in range:
    ///             block = BlockMap[x, y, z].
    /// </code>
    /// </summary>
    /// <param name="position">Chunk-relative position of the block</param>
    /// <param name="block">Block to set</param>
    /// <param name="oldBlock">Old block at the given position</param>
    /// <param name="delayedMeshDirtying">If true, the chunk mesh will not be marked dirty until <see cref="ExecuteDelayedMeshDirtying"/> is called.
    /// Has only effect if <see cref="_currentState"/> is <see cref="ChunkState.READY"/></param>
    public void SetBlockState(ChunkBlockPosition position, BlockState block, out BlockState oldBlock, bool delayedMeshDirtying)
    {
        _blockStorage.SetBlock(position, block, out oldBlock);
        _containsRenderedBlocks = _blockStorage.RenderedBlockCount > 0;

        bool isChunkReady = _currentState == ChunkState.READY;
        bool renderedBlockChanged = oldBlock.IsRendered || block.IsRendered;
        
        // Only consider re-meshing if the chunk has been meshed before, is not meshed currently, and a rendered block was changed.
        // NOTE: MIGHT cause mesh desync issues when settings blocks on chunk borders, but this needs to be tested.
        // If multiple blocks are set with delayedMeshDirtying=false, only the first block would update the _neighboursToMeshDirty mask.
        // This is because SetSelfAndNeighboursMeshDirty would be called for the first block, changing chunk state and changing _currentState.
        if (!isChunkReady || !renderedBlockChanged)
            return;
        
        // Cache the neighbours that would be affected by this change to dirty them,
        // either when ExecuteDelayedMeshDirtying is called or if delayedMeshDirtying is false
        ChunkOffsets.NeighbourOffsetFlags affectedNeighbours = ChunkOffsets.CalculateNeighboursFromOtherChunks(position);
        _neighboursToMeshDirty |= affectedNeighbours;
        
        if (delayedMeshDirtying)
            return;
        
        SetSelfAndNeighboursMeshDirty(_neighboursToMeshDirty);
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// <code>
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </code>
    /// </summary>
    public BlockState GetBlockState(ChunkBlockPosition position)
    {
        return _blockStorage.GetBlock(position);
    }


    /// <summary>
    /// Sets the chunk mesh dirty, causing it to be regenerated.
    /// Should be called after calling <see cref="SetBlockState"/> with delayedMeshDirtying set to true.
    /// </summary>
    public void ExecuteDelayedMeshDirtying()
    {
        // Dirty the neighbours that were affected by SetBlockState with delayedMeshDirtying set to true
        SetSelfAndNeighboursMeshDirty(_neighboursToMeshDirty);
    }


    private void SetSelfAndNeighboursMeshDirty(ChunkOffsets.NeighbourOffsetFlags neighbours)
    {
        SetMeshDirty();

        DirtyNeighbours(neighbours);
    }


    private void DirtyNeighbours(ChunkOffsets.NeighbourOffsetFlags flags)
    {
        if (flags == ChunkOffsets.NeighbourOffsetFlags.None)
            return;
        
        foreach (Vector3i vector in ChunkOffsets.OffsetsAsChunkVectors(flags))
        {
            Vector3i neighbourPos = Position + vector;
            Chunk? neighbourChunk = GameWorld.CurrentGameWorld.RegionManager.GetChunkAt(neighbourPos);

            neighbourChunk?.SetMeshDirty();
        }
        
        _neighboursToMeshDirty = ChunkOffsets.NeighbourOffsetFlags.None;
    }


    private void SetMeshDirty()
    {
        if (!_containsRenderedBlocks)
            return;

        if (_currentState != ChunkState.WAITING_FOR_MESHING)
            ChangeState(ChunkState.WAITING_FOR_MESHING);
    }


    private void ChangeState(ChunkState newState)
    {
        if (_currentState == newState)
        {
            Logger.LogWarning($"Chunk {Position} tried to change to state {newState} but is already in that state.");
            return;
        }
        ChunkState previousState = _currentState;
        OnExitState(previousState);
        _currentState = newState;
        OnEnterState(newState);
    }


    private void OnEnterState(ChunkState newState)
    {
        switch (newState)
        {
            case ChunkState.UNINITIALIZED:
                break;
            case ChunkState.GENERATING_TERRAIN:
                Interlocked.Increment(ref _currentJobId);
                GlobalThreadPool.DispatchJob(new GenerationJob(_currentJobId, this, () => ChangeState(ChunkState.GENERATING_DECORATION)), WorkItemPriority.Normal);
                break;
            case ChunkState.GENERATING_DECORATION:
                ChangeState(ChunkState.GENERATING_LIGHTING);
                break;
            case ChunkState.GENERATING_LIGHTING:
                ChangeState(ChunkState.WAITING_FOR_MESHING);
                break;
            case ChunkState.WAITING_FOR_MESHING:
                if (_containsRenderedBlocks)
                {
                    Debug.Assert(HasBeenGenerated, "Chunk contains rendered blocks but has not been generated.");
                    
                    // If the chunk contains rendered blocks, wait for neighbours to be generated before meshing
                    if (AreAllNeighboursGenerated(false))
                        ChangeState(ChunkState.MESHING);
                }
                else
                {
                    // Skip meshing if the chunk is empty
                    ChangeState(ChunkState.READY);
                }
                break;
            case ChunkState.MESHING:
                Interlocked.Increment(ref _currentJobId);
                GlobalThreadPool.DispatchJob(new MeshingJob(_currentJobId, this, () => ChangeState(ChunkState.READY)), WorkItemPriority.High);
                break;
            case ChunkState.READY:
                _hasBeenMeshed = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }


    private void ExecuteCurrentState()
    {
        switch (_currentState)
        {
            case ChunkState.UNINITIALIZED:
                break;
            case ChunkState.GENERATING_TERRAIN:
                break;
            case ChunkState.GENERATING_DECORATION:
                break;
            case ChunkState.GENERATING_LIGHTING:
                break;
            case ChunkState.WAITING_FOR_MESHING:
                Debug.Assert(HasBeenGenerated, "Chunk contains rendered blocks but has not been generated.");
                if (AreAllNeighboursGenerated(false))
                    ChangeState(ChunkState.MESHING);
                break;
            case ChunkState.MESHING:
                break;
            case ChunkState.READY:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void OnExitState(ChunkState previousState)
    {
        switch (previousState)
        {
            case ChunkState.UNINITIALIZED:
                break;
            case ChunkState.GENERATING_TERRAIN:
                break;
            case ChunkState.GENERATING_DECORATION:
                break;
            case ChunkState.GENERATING_LIGHTING:
                break;
            case ChunkState.WAITING_FOR_MESHING:
                break;
            case ChunkState.MESHING:
                break;
            case ChunkState.READY:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(previousState), previousState, null);
        }
    }


    /// <summary>
    /// Checks if all neighbouring chunks of this chunk are generated.
    /// </summary>
    /// <param name="excludeMissingChunks">If true, chunks that are not loaded are excluded from neighbourhood checks</param>
    /// <returns>True if all neighbouring chunks are generated, false otherwise</returns>
    private bool AreAllNeighboursGenerated(bool excludeMissingChunks)
    {
        foreach (Vector3i chunkOffset in ChunkOffsets.ChunkNeighbourOffsets)
        {
            Vector3i neighbourPos = Position + chunkOffset;
            Chunk? neighbourChunk = GameWorld.CurrentGameWorld.RegionManager.GetChunkAt(neighbourPos);

            if (neighbourChunk == null)
            {
                if (!excludeMissingChunks)
                    return false;
            }
            else
            {
                if (!neighbourChunk.HasBeenGenerated)
                    return false;
            }
        }
        
        return true;
    }


#if DEBUG
    private void DebugDraw()
    {
        if (!ClientConfig.DebugModeConfig.RenderChunkMeshState)
            return;
        
        const float halfAChunk = Constants.CHUNK_SIDE_LENGTH / 2f;
        Vector3 centerOffset = new(halfAChunk, 0, halfAChunk);

        Color4 color = Color4.White;
        switch (_currentState)
        {
            case ChunkState.UNINITIALIZED:
                break;
            case ChunkState.GENERATING_TERRAIN:
                color = Color4.Red;
                break;
            case ChunkState.GENERATING_DECORATION:
                color = Color4.Blue;
                break;
            case ChunkState.GENERATING_LIGHTING:
                color = Color4.Yellow;
                break;
            case ChunkState.WAITING_FOR_MESHING:
                color = Color4.Orange;
                break;
            case ChunkState.MESHING:
                color = Color4.Cyan;
                break;
            case ChunkState.READY:
                color = Color4.Green;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, color);
    }
#endif
}