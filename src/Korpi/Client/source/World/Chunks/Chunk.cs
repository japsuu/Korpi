using System.Diagnostics;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Meshing;
using Korpi.Client.Meshing.Jobs;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Rendering.Chunks;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World.Chunks.BlockStorage;
using KorpiEngine.Core.Logging;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

public class Chunk
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Chunk));

    /// <summary>
    /// The chunk column this chunk belongs to.
    /// </summary>
    private readonly IChunkColumn _column;
    private readonly IBlockStorage _blockStorage = new PaletteBlockStorage();
    private readonly ChunkRenderManager _renderManager;

    private long _currentJobId;
    private bool _hasBeenMeshed;
    private ChunkMeshState _currentMeshState;
    private ChunkOffsets.NeighbourOffsetFlags _neighboursToMeshDirty;


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

    /// <summary>
    /// True if this chunk contains rendered blocks, false otherwise.
    /// </summary>
    internal bool ContainsRenderedBlocks;

    /// <summary>
    /// True if this chunk has been generated, false otherwise.
    /// </summary>
    internal bool HasBeenGenerated;
    
    private bool IsWaitingForMeshing => _currentMeshState is ChunkMeshState.MESHING or ChunkMeshState.WAITING_FOR_NEIGHBOURS;

    /// <summary>
    /// The chunk column this chunk belongs to.
    /// </summary>
    public IChunkColumn Column => _column;


    public Chunk(IChunkColumn column, int height)
    {
        _column = column;
        Position = new Vector3i(column.Position.X, height, column.Position.Y);
        Top = Position.Y + Constants.CHUNK_SIDE_LENGTH - 1;
        Bottom = Position.Y;

        HasBeenGenerated = false;
        ContainsRenderedBlocks = false;
        _renderManager = new ChunkRenderManager();
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

        // Skip the transparent pass if the chunk doesn't contain any transparent blocks
        if (_blockStorage.TranslucentBlockCount == 0 && pass == RenderPass.Transparent)
            return;

        // Frustum check.
#if DEBUG
        // If in debug mode, allow the player to toggle frustum culling on/off
        if (ClientConfig.Rendering.Debug.DoFrustumCulling)
        {
            Frustum cameraViewFrustum = ClientConfig.Rendering.Debug.OnlyPlayerFrustumCulling
                ? PlayerEntity.LocalPlayerEntity.Camera.ViewFrustum
                : Camera.RenderingCamera.ViewFrustum;

            if (!IsOnFrustum(cameraViewFrustum))
                return;
        }
#else
        if (!IsOnFrustum(PlayerEntity.LocalPlayerEntity.Camera.ViewFrustum))
            return;
#endif
        
        _renderManager.RenderMesh(pass);

#if DEBUG
        DebugDraw();
#endif
    }
    
    
    public void UpdateMesh(ChunkMesh mesh)
    {
        _renderManager.AddOrUpdateMesh(mesh);
    }


    public bool IsOnFrustum(Frustum viewFrustum)
    {
        Vector3 min = Position;
        Vector3 max = Position + new Vector3(Constants.CHUNK_SIDE_LENGTH);

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
        _renderManager.DeleteMesh();
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
    /// <param name="position">Chunk-relative position of the block</param>
    /// <param name="block">Block to set</param>
    /// <param name="oldBlock">Old block at the given position</param>
    /// <param name="delayedMeshDirtying">If true, the chunk mesh will not be marked dirty until <see cref="ExecuteDelayedMeshDirtying"/> is called.
    /// Has only effect if <see cref="_currentMeshState"/> is <see cref="ChunkGenerationState.READY"/></param>
    public void SetBlockState(ChunkBlockPosition position, BlockState block, out BlockState oldBlock, bool delayedMeshDirtying)
    {
        _blockStorage.SetBlock(position, block, out oldBlock);
        ContainsRenderedBlocks = _blockStorage.RenderedBlockCount > 0;

        bool renderedBlockChanged = oldBlock.IsRendered || block.IsRendered;

        // Only consider re-meshing if the chunk has been meshed before, is not meshed currently, and a rendered block was changed.
        // NOTE: MIGHT cause mesh desync issues when settings blocks on chunk borders, but this needs to be tested.
        // If multiple blocks are set with delayedMeshDirtying=false, only the first block would update the _neighboursToMeshDirty mask.
        // This is because SetSelfAndNeighboursMeshDirty would be called for the first block, changing chunk state and changing _currentMeshState.
        if (!HasBeenGenerated || IsWaitingForMeshing || !renderedBlockChanged)
            return;

        // Cache the neighbours that would be affected by this change to dirty them,
        // either when ExecuteDelayedMeshDirtying is called or if delayedMeshDirtying is false
        ChunkOffsets.NeighbourOffsetFlags affectedNeighbours = ChunkOffsets.CalculateNeighboursFromOtherChunks(position);
        _neighboursToMeshDirty |= affectedNeighbours;

        if (delayedMeshDirtying)
            return;
        
        // If the chunk doesn't contain any rendered blocks anymore, delete the mesh if it exists and skip meshing self.
        if (!ContainsRenderedBlocks)
        {
            ChangeState(ChunkMeshState.UNINITIALIZED);
            _renderManager.DeleteMesh();
            DirtyNeighbours(_neighboursToMeshDirty);
            return;
        }

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


    private void ChangeState(ChunkMeshState newState)
    {
        Debug.Assert(SystemInfo.MainThreadId == Environment.CurrentManagedThreadId, "Chunk state should only be changed on the main thread.");
        Debug.Assert(HasBeenGenerated, "Chunk is trying to change the mesh state before it has been generated.");
        
        if (_currentMeshState == newState && newState != ChunkMeshState.UNINITIALIZED)
        {
            Logger.Warn($"Chunk {Position} tried to change to state {newState} but is already in that state.");
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
                if (ContainsRenderedBlocks)
                {
                    // If the chunk contains rendered blocks, wait for neighbours to be generated before meshing
                    if (_column.AreAllNeighboursGenerated(false))
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
                GlobalJobPool.DispatchJob(new MeshingJob(_currentJobId, this, () => ChangeState(ChunkMeshState.READY), _hasBeenMeshed));
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
                if (_column.AreAllNeighboursGenerated(false))
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
            if (neighbourPos.Y < 0 || neighbourPos.Y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
                continue;
            Chunk? neighbourChunk = GameWorld.CurrentGameWorld.ChunkManager.GetChunkAt(neighbourPos);

            neighbourChunk?.SetMeshDirty();
        }

        _neighboursToMeshDirty = ChunkOffsets.NeighbourOffsetFlags.None;
    }


    internal void SetMeshDirty()
    {
        if (!ContainsRenderedBlocks)
            return;

        if (_currentMeshState != ChunkMeshState.WAITING_FOR_NEIGHBOURS)
            ChangeState(ChunkMeshState.WAITING_FOR_NEIGHBOURS);
    }


    internal void Reset()
    {
        _blockStorage.Clear();
        ChangeState(ChunkMeshState.UNINITIALIZED);
        ContainsRenderedBlocks = false;
        HasBeenGenerated = false;
        _hasBeenMeshed = false;
    }


#if DEBUG
    private void DebugDraw()
    {
        if (!ClientConfig.Rendering.Debug.RenderChunkMeshState)
            return;

        const float halfAChunk = Constants.CHUNK_SIDE_LENGTH / 2f;
        Vector3 centerOffset = new(halfAChunk, 0, halfAChunk);

        Color4 color = _currentMeshState switch
        {
            ChunkMeshState.UNINITIALIZED => Color4.Red,
            ChunkMeshState.WAITING_FOR_NEIGHBOURS => Color4.Orange,
            ChunkMeshState.MESHING => Color4.Yellow,
            ChunkMeshState.READY => Color4.Green,
            _ => throw new ArgumentOutOfRangeException()
        };
        Debugging.Drawing.DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, color);
    }
#endif
}