using System.Diagnostics;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Meshing.Jobs;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Rendering.Chunks;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World.Chunks.BlockStorage;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

public class SubChunk
{
    private static readonly Logging.IKorpiLogger Logger = Logging.LogFactory.GetLogger(typeof(SubChunk));

    private readonly IBlockStorage _blockStorage = new BlockPalette();

    private long _currentJobId;
    private bool _hasBeenMeshed;
    private ChunkMeshState _currentMeshState;
    private ChunkOffsets.NeighbourOffsetFlags _neighboursToMeshDirty;

    /// <summary>
    /// Position of this subchunk in the world.
    /// </summary>
    public readonly Vector3i Position;

    /// <summary>
    /// Position of the chunk which contains this subchunk in the world.
    /// </summary>
    public readonly Vector2i ChunkPosition;

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

    /// <summary>
    /// True if this chunk contains rendered blocks, false otherwise.
    /// </summary>
    private bool _containsRenderedBlocks;

    private bool HasBeenGenerated => _currentMeshState > ChunkMeshState.UNINITIALIZED;


    public SubChunk(Vector3i position)
    {
        Position = position;
        ChunkPosition = new Vector2i(position.X, position.Z);
        Top = position.Y + Constants.SUBCHUNK_SIDE_LENGTH - 1;
        Bottom = position.Y;

        _containsRenderedBlocks = false;
        ThreadLock = new ReaderWriterLockSlim();
    }


    public void Tick()
    {
        ExecuteCurrentState();
    }


    public void Draw(RenderPass pass)
    {
        if (!_hasBeenMeshed)
            return;

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
        if (ChunkRendererStorage.TryGetRenderer(Position, out ChunkRenderer? mesh))
            mesh!.Draw(pass);

#if DEBUG
        DebugDraw();
#endif
    }


    public bool IsOnFrustum(Frustum viewFrustum)
    {
        Vector3 min = Position;
        Vector3 max = Position + new Vector3(Constants.SUBCHUNK_SIDE_LENGTH);

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


    public void Load()
    {
    }


    public void Unload()
    {
        ChunkRendererStorage.RemoveChunkMesh(Position);
    }


    /// <summary>
    /// Sets the block at the given position and returns the old block.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// <code>
    /// for z in range:
    ///     for y in range:
    ///         for x in range:
    ///             block = BlockMap[x, y, z].
    /// </code>
    /// </summary>
    /// <param name="position">SubChunk-relative position of the block</param>
    /// <param name="block">Block to set</param>
    /// <param name="oldBlock">Old block at the given position</param>
    /// <param name="delayedMeshDirtying">If true, the chunk mesh will not be marked dirty until <see cref="ExecuteDelayedMeshDirtying"/> is called.
    /// Has only effect if <see cref="_currentMeshState"/> is <see cref="ChunkGenerationState.READY"/></param>
    public void SetBlockState(SubChunkBlockPosition position, BlockState block, out BlockState oldBlock, bool delayedMeshDirtying)
    {
        _blockStorage.SetBlock(position, block, out oldBlock);
        _containsRenderedBlocks = _blockStorage.RenderedBlockCount > 0;

        bool isChunkReady = _currentMeshState == ChunkMeshState.READY;
        bool renderedBlockChanged = oldBlock.IsRendered || block.IsRendered;

        // Only consider re-meshing if the chunk has been meshed before, is not meshed currently, and a rendered block was changed.
        // NOTE: MIGHT cause mesh desync issues when settings blocks on chunk borders, but this needs to be tested.
        // If multiple blocks are set with delayedMeshDirtying=false, only the first block would update the _neighboursToMeshDirty mask.
        // This is because SetSelfAndNeighboursMeshDirty would be called for the first block, changing chunk state and changing _currentMeshState.
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
    /// Gets the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// <code>
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </code>
    /// </summary>
    public BlockState GetBlockState(SubChunkBlockPosition position)
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


    internal void ChangeState(ChunkMeshState newState)
    {
        if (_currentMeshState == newState && newState != ChunkMeshState.UNINITIALIZED)
        {
            Logger.Warn($"SubChunk {Position} tried to change to state {newState} but is already in that state.");
            return;
        }

        ChunkMeshState previousState = _currentMeshState;
        OnExitState(previousState);
        _currentMeshState = newState;
        OnEnterState(newState);
    }


    private void OnEnterState(ChunkMeshState newState)
    {
        switch (newState)
        {
            case ChunkMeshState.UNINITIALIZED:
                break;
            case ChunkMeshState.WAITING_FOR_NEIGHBOURS:
                if (_containsRenderedBlocks)
                {
                    Debug.Assert(HasBeenGenerated, "SubChunk contains rendered blocks but has not been generated.");

                    // If the chunk contains rendered blocks, wait for neighbours to be generated before meshing
                    if (Chunk.AreAllNeighboursGenerated(ChunkPosition, false))
                        ChangeState(ChunkMeshState.MESHING);
                }
                else
                {
                    // Skip meshing if the chunk is empty
                    ChangeState(ChunkMeshState.READY);
                }

                break;
            case ChunkMeshState.MESHING:
                Interlocked.Increment(ref _currentJobId);
                GlobalThreadPool.DispatchJob(new MeshingJob(_currentJobId, this, () => ChangeState(ChunkMeshState.READY)), WorkItemPriority.High);
                break;
            case ChunkMeshState.READY:
                _hasBeenMeshed = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }


    private void ExecuteCurrentState()
    {
        switch (_currentMeshState)
        {
            case ChunkMeshState.UNINITIALIZED:
                break;
            case ChunkMeshState.WAITING_FOR_NEIGHBOURS:
                Debug.Assert(HasBeenGenerated, "SubChunk contains rendered blocks but has not been generated.");
                if (Chunk.AreAllNeighboursGenerated(ChunkPosition, false))
                    ChangeState(ChunkMeshState.MESHING);
                break;
            case ChunkMeshState.MESHING:
                break;
            case ChunkMeshState.READY:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void OnExitState(ChunkMeshState previousState)
    {
        switch (previousState)
        {
            case ChunkMeshState.UNINITIALIZED:
                break;
            case ChunkMeshState.WAITING_FOR_NEIGHBOURS:
                break;
            case ChunkMeshState.MESHING:
                break;
            case ChunkMeshState.READY:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(previousState), previousState, null);
        }
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
            if (neighbourPos.Y < 0 || neighbourPos.Y >= Constants.CHUNK_HEIGHT_BLOCKS)
                continue;
            SubChunk? neighbourChunk = GameWorld.CurrentGameWorld.ChunkManager.GetSubChunkAt(neighbourPos);

            neighbourChunk?.SetMeshDirty();
        }

        _neighboursToMeshDirty = ChunkOffsets.NeighbourOffsetFlags.None;
    }


    internal void SetMeshDirty()
    {
        if (!_containsRenderedBlocks)
            return;

        if (_currentMeshState != ChunkMeshState.WAITING_FOR_NEIGHBOURS)
            ChangeState(ChunkMeshState.WAITING_FOR_NEIGHBOURS);
    }


    internal void Reset()
    {
        _blockStorage.Clear();
        ChangeState(ChunkMeshState.UNINITIALIZED);
    }


#if DEBUG
    private void DebugDraw()
    {
        if (!ClientConfig.DebugModeConfig.RenderChunkMeshState)
            return;

        const float halfAChunk = Constants.SUBCHUNK_SIDE_LENGTH / 2f;
        Vector3 centerOffset = new(halfAChunk, 0, halfAChunk);

        Color4 color = _currentMeshState switch
        {
            ChunkMeshState.UNINITIALIZED => Color4.Red,
            ChunkMeshState.WAITING_FOR_NEIGHBOURS => Color4.Orange,
            ChunkMeshState.MESHING => Color4.Yellow,
            ChunkMeshState.READY => Color4.Green,
            _ => throw new ArgumentOutOfRangeException()
        };
        Debugging.Drawing.DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.SUBCHUNK_SIDE_LENGTH, color);
    }
#endif
}