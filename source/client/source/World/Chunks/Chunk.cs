using System.Diagnostics;
using Korpi.Client.Configuration;
using Korpi.Client.Generation.Jobs;
using Korpi.Client.Rendering;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Chunks;

/// <summary>
/// Vertical column of sub-chunks.
/// </summary>
public class Chunk
{
    private static readonly Logging.IKorpiLogger Logger = Logging.LogFactory.GetLogger(typeof(Chunk));
    
    private ChunkGenerationState _currentState = ChunkGenerationState.UNINITIALIZED;
    private long _currentJobId;
    private readonly SubChunk[] _subchunks;
    private readonly ChunkHeightmap _heightmap;

    public readonly Vector2i Position;
    
    /// <summary>
    /// Lock used to synchronize access to this chunk.
    /// </summary>
    public readonly ReaderWriterLockSlim ThreadLock;
    
    /// <summary>
    /// Id of the job last executed on this chunk.
    /// </summary>
    public long CurrentJobId => Interlocked.Read(ref _currentJobId);
        
        
    public Chunk(Vector2i position)
    {
        Position = position;
        _subchunks = new SubChunk[Constants.CHUNK_HEIGHT_SUBCHUNKS];
        _heightmap = new ChunkHeightmap();
        ThreadLock = new ReaderWriterLockSlim();
        
        GameWorld.WorldEventPublished += WorldEventHandler;
    }
        
    
    /// <returns>If this chunk can be safely unloaded.</returns>
    public bool ReadyToUnload() => true;
    
    
    /// <returns>The highest block at the given x and z coordinates. -1 if no blocks are found.</returns>
    public int GetHighestBlock(int x, int z) => _heightmap.GetHighestBlock(x, z);
        
    
    /// <returns>The subchunk at the given height.</returns>
    public SubChunk GetSubchunkAtHeight(int y) => _subchunks[y / Constants.SUBCHUNK_SIDE_LENGTH];


    /// <summary>
    /// Loads this chunk, and creates and loads all contained sub-chunks.
    /// </summary>
    public void Load()
    {
        for (int i = Constants.CHUNK_HEIGHT_SUBCHUNKS - 1; i >= 0; i--)
        {
            SubChunk subChunk = new(new Vector3i(Position.X, i * Constants.SUBCHUNK_SIDE_LENGTH, Position.Y));
            subChunk.Load();
            _subchunks[i] = subChunk;
        }

        // if (IsOnFrustum(PlayerEntity.LocalPlayerEntity.Camera.ViewFrustum))
        // if (Vector3.Distance(Camera.RenderingCamera.Position, new Vector3(Position.X, Camera.RenderingCamera.Position.Y, Position.Y)) < Constants.SUBCHUNK_SIDE_LENGTH * 3)
        //     ChangeState(ChunkGenerationState.GENERATING_TERRAIN);
        ChangeState(ChunkGenerationState.GENERATING_TERRAIN);
    }


    /// <summary>
    /// Ticks this chunk, and all contained sub-chunks.
    /// </summary>
    public void Tick()
    {
        ExecuteCurrentState();
        
        foreach (SubChunk subChunk in _subchunks)
            subChunk.Tick();
    }
    
    
    /// <summary>
    /// Draws this chunk, and all contained sub-chunks.
    /// </summary>
    /// <param name="pass">The render pass to draw with.</param>
    public void Draw(RenderPass pass)
    {
        //TODO: Render the subchunk the player is in, first.
        for (int i = _subchunks.Length - 1; i >= 0; i--)
        {
            SubChunk subChunk = _subchunks[i];
            subChunk.Draw(pass);
        }
    }
        
    
    /// <summary>
    /// Unloads this chunk, and all contained sub-chunks.
    /// </summary>
    public void Unload()
    {
        foreach (SubChunk chunk in _subchunks)
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
        Debug.Assert(x >= 0 && x < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_HEIGHT_BLOCKS);
        Debug.Assert(z >= 0 && z < Constants.SUBCHUNK_SIDE_LENGTH);
        
        int arrayIndex = y / Constants.SUBCHUNK_SIDE_LENGTH;
        SubChunkBlockPosition pos = new(x, y % Constants.SUBCHUNK_SIDE_LENGTH, z);
        _subchunks[arrayIndex].SetBlockState(pos, block, out oldBlock, delayedMeshDirtying);
        
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
        Debug.Assert(x >= 0 && x < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_HEIGHT_BLOCKS);
        Debug.Assert(z >= 0 && z < Constants.SUBCHUNK_SIDE_LENGTH);
        
        int arrayIndex = y / Constants.SUBCHUNK_SIDE_LENGTH;
        SubChunkBlockPosition pos = new(x, y % Constants.SUBCHUNK_SIDE_LENGTH, z);
        return _subchunks[arrayIndex].GetBlockState(pos);
    }
    
    
    public bool IsOnFrustum(Frustum viewFrustum)
    {
        Vector3 position = new Vector3(Position.X, 0, Position.Y);
        Vector3 min = position;
        Vector3 max = position + new Vector3(Constants.SUBCHUNK_SIDE_LENGTH, Constants.CHUNK_HEIGHT_BLOCKS, Constants.SUBCHUNK_SIDE_LENGTH);

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
            Logger.Warn($"Chunk {Position} tried to change to state {newState} but is already in that state.");
            return;
        }
        ChunkGenerationState previousState = _currentState;
        OnExitState(previousState);
        _currentState = newState;
        OnEnterState(newState);
    }


    private void OnEnterState(ChunkGenerationState newState)
    {
        switch (newState)
        {
            case ChunkGenerationState.UNINITIALIZED:
                break;
            case ChunkGenerationState.GENERATING_TERRAIN:
                Interlocked.Increment(ref _currentJobId);
                GlobalThreadPool.DispatchJob(new GenerationJob(_currentJobId, this, () => ChangeState(ChunkGenerationState.GENERATING_DECORATION)), WorkItemPriority.Normal);
                break;
            case ChunkGenerationState.GENERATING_DECORATION:
                ChangeState(ChunkGenerationState.GENERATING_LIGHTING);
                break;
            case ChunkGenerationState.GENERATING_LIGHTING:
                ChangeState(ChunkGenerationState.READY);
                break;
            case ChunkGenerationState.READY:
                for (int i = _subchunks.Length - 1; i >= 0; i--)
                    _subchunks[i].SetMeshDirty();
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
                for (int i = _subchunks.Length - 1; i >= 0; i--)
                {
                    SubChunk subChunk = _subchunks[i];
                    subChunk.SetMeshDirty();
                }

                break;
            case WorldEvent.LOAD_REGION_CHANGED:
                break;
            case WorldEvent.REGENERATE_ALL_CHUNKS:
                for (int i = _subchunks.Length - 1; i >= 0; i--)
                {
                    SubChunk subChunk = _subchunks[i];
                    subChunk.Reset();
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
    /// <param name="centerPosition">Position of the chunk which neighbours are checked</param>
    /// <param name="excludeMissingChunks">If true, chunks that are not loaded are excluded from neighbourhood checks</param>
    /// <returns>True if all neighbouring chunks are generated, false otherwise</returns>
    internal static bool AreAllNeighboursGenerated(Vector2i centerPosition, bool excludeMissingChunks)
    {
        foreach (Vector2i chunkOffset in ChunkOffsets.ChunkNeighbourOffsets)
        {
            Vector2i neighbourPos = centerPosition + chunkOffset;
            
            Chunk? neighbourChunk = GameWorld.CurrentGameWorld.ChunkManager.GetChunkAt(neighbourPos);

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