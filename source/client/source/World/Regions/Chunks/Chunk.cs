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

/// <summary>
/// Represents the state of the chunk.
/// </summary>
public enum ChunkState
{
    /// <summary>
    /// The chunk is uninitialized.
    /// </summary>
    UNINITIALIZED,
    
    /// <summary>
    /// The chunk is queued for terrain generation.
    /// </summary>
    GENERATING_TERRAIN,
    
    /// <summary>
    /// The chunk is queued for decoration generation.
    /// </summary>
    GENERATING_DECORATION,
        
    /// <summary>
    /// The chunk is queued for lighting generation.
    /// </summary>
    GENERATING_LIGHTING,

    /// <summary>
    /// The chunk is waiting for neighbouring chunks to be generated.
    /// </summary>
    WAITING_FOR_MESHING,

    /// <summary>
    /// The chunk is queued for meshing.
    /// </summary>
    MESHING,

    /// <summary>
    /// The chunk is ready.
    /// </summary>
    READY
}

public class Chunk
{
    private readonly IBlockStorage _blockStorage = new BlockPalette();

    private long _currentJobId;
    private bool _containsRenderedBlocks;
    
    // State machine
    private ChunkState _currentState;

    public readonly ReaderWriterLockSlim ThreadLock;
    public readonly Vector3i Position;
    public readonly int Top;
    public readonly int Bottom;

    public long CurrentJobId => Interlocked.Read(ref _currentJobId);
    public bool HasBeenGenerated => _currentState >= ChunkState.WAITING_FOR_MESHING;
    public bool HasBeenMeshed { get; private set; }


    public Chunk(Vector3i position)
    {
        Position = position;
        Top = position.Y + Constants.CHUNK_SIDE_LENGTH - 1;
        Bottom = position.Y;
        
        _containsRenderedBlocks = false;
        ThreadLock = new ReaderWriterLockSlim();
    }


    private void ChangeState(ChunkState newState)
    {
        if (_currentState == newState)
        {
            Logger.LogWarning($"Chunk {Position} tried to change to state {newState} but is already in that state.");
            return;
        }
        ChunkState previousState = _currentState;
        OnExitState(newState, previousState);
        _currentState = newState;
        OnEnterState(newState, previousState);
    }


    private void OnEnterState(ChunkState newState, ChunkState previousState)
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
                HasBeenMeshed = true;
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


    private void OnExitState(ChunkState newState, ChunkState previousState)
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
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }


    public void Tick()
    {
        ExecuteCurrentState();
    }


    public void Draw()
    {
        if (!HasBeenMeshed)
            return;

        if (ChunkRendererStorage.TryGetRenderer(Position, out ChunkRenderer? mesh))
            mesh!.Draw();
        
#if DEBUG
        if (ClientConfig.DebugModeConfig.RenderChunkMeshState)
        {
            const float halfAChunk = Constants.CHUNK_SIDE_LENGTH / 2f;
            Vector3 centerOffset = new(halfAChunk, 0, halfAChunk);

            // if (_containsRenderedBlocks)
            // {
            //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Green);
            // }
            // else
            // {
            //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Red);
            // }
            // return;
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

            // if (CoordinateUtils.WorldToChunk(Camera.RenderingCamera.Position) == Position)
            //     DebugTextWindow.AddFrameText(Position + new Vector3(halfAChunk, halfAChunk, halfAChunk), $"HighestRenderedBlockY = {HighestRenderedBlockY}");
        }
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


    public void SetMeshDirty()
    {
        if (!_containsRenderedBlocks)
            return;

        if (_currentState != ChunkState.WAITING_FOR_MESHING)
            ChangeState(ChunkState.WAITING_FOR_MESHING);
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


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// <code>
    /// for z in range:
    ///     for y in range:
    ///         for x in range:
    ///             block = BlockMap[x, y, z].
    /// </code>
    /// Thread safe.
    /// </summary>
    public bool SetBlockState(ChunkBlockPosition position, BlockState block, out BlockState oldBlock)
    {
        _blockStorage.SetBlock(position, block, out oldBlock);

        // If the chunk has been meshed and a rendered block was changed, mark the chunk mesh as dirty.
        //bool shouldDirtyMesh = _generationState == ChunkGenerationState.READY && _meshState != ChunkMeshState.MESHING && (oldBlock.IsRendered || block.IsRendered);
        bool shouldDirtyMesh = _currentState == ChunkState.READY && (oldBlock.IsRendered || block.IsRendered);

        if (shouldDirtyMesh)
            ChangeState(ChunkState.WAITING_FOR_MESHING);

        _containsRenderedBlocks = _blockStorage.RenderedBlockCount > 0;

        return shouldDirtyMesh;
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(ChunkBlockPosition position)
    {
        return _blockStorage.GetBlock(position);
    }
}