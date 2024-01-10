using Korpi.Client.Configuration;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Generation.Jobs;
using Korpi.Client.Logging;
using Korpi.Client.Meshing.Jobs;
using Korpi.Client.Threading.Pooling;
using Korpi.Client.World.Regions.Chunks.Blocks;
using Korpi.Client.World.Regions.Chunks.BlockStorage;
using OpenTK.Mathematics;

namespace Korpi.Client.World.Regions.Chunks;

public class Chunk
{
    /// <summary>
    /// Represents the state of the chunk.
    /// </summary>
    private enum ChunkGenerationState    //TODO: Add "Decorating" state
    {
        /// <summary>
        /// The chunk has not been generated yet.
        /// </summary>
        NONE = 0,
        
        /// <summary>
        /// The chunk is queued for generation.
        /// </summary>
        GENERATING = 1,
        
        /// <summary>
        /// The chunk has been generated.
        /// </summary>
        READY = 2
    }

    /// <summary>
    /// Represents the state of the chunk mesh.
    /// </summary>
    private enum ChunkMeshState
    {
        /// <summary>
        /// The chunk has not been meshed.
        /// </summary>
        NONE = 0,
        
        /// <summary>
        /// The chunk is waiting for neighbouring chunks to be generated.
        /// </summary>
        WAITING_FOR_NEIGHBOURS_TO_GENERATE = 1,
        
        /// <summary>
        /// The chunk is queued for meshing.
        /// </summary>
        MESHING = 2,
        
        /// <summary>
        /// The chunk has been meshed.
        /// </summary>
        READY = 4
    }


    private readonly IBlockStorage _blockStorage = new BlockPalette();
    private readonly object _blockStorageLock = new();
    private readonly Action _generateJobCallback;
    private readonly Action _meshJobCallback;

    private bool _containsRenderedBlocks;
    private bool _isLoaded;
    private long _currentJobId;
    private ChunkGenerationState _generationState;
    private ChunkMeshState _meshState;
    
    public readonly ReaderWriterLockSlim ThreadLock;
    public readonly Vector3i Position;
    public readonly int Top;
    public readonly int Bottom;
    
    public bool IsGenerated => _generationState == ChunkGenerationState.READY;
    public bool ShouldBeRendered => IsGenerated && _meshState >= ChunkMeshState.MESHING;
    public long CurrentJobId => Interlocked.Read(ref _currentJobId);


    public Chunk(Vector3i position)     //TODO: Isolate state to a separate class
    {
        Position = position;
        Top = position.Y + Constants.CHUNK_SIDE_LENGTH - 1;
        Bottom = position.Y;
        _containsRenderedBlocks = false;
        ThreadLock = new ReaderWriterLockSlim();
        
        // Set callbacks
        _generateJobCallback = OnGenerated;
        _meshJobCallback = OnMeshed;
        
        // Set state
        _generationState = ChunkGenerationState.NONE;
        _meshState = ChunkMeshState.NONE;
    }


    public void Tick()
    {
        if (!_isLoaded)
            throw new InvalidOperationException("Tried to tick an unloaded chunk!");
        
        if (_meshState == ChunkMeshState.WAITING_FOR_NEIGHBOURS_TO_GENERATE)
        {
            if (CanBeMeshed())
            {
                EnqueueForMeshing();
            }
        }

#if DEBUG
        if (Configuration.ClientConfig.DebugModeConfig.RenderChunkMeshState)
        {
            const float halfAChunk = Constants.CHUNK_SIDE_LENGTH / 2f;
            Vector3 centerOffset = new Vector3(halfAChunk, 0, halfAChunk);
            // if (_containsRenderedBlocks)
            // {
            //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Green);
            // }
            // else
            // {
            //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Red);
            // }
            // return;
            switch (_meshState)
            {
                case ChunkMeshState.NONE:
                    DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Red);
                    break;
                case ChunkMeshState.WAITING_FOR_NEIGHBOURS_TO_GENERATE:
                    DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Blue);
                    break;
                case ChunkMeshState.MESHING:
                    DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Yellow);
                    break;
                // case ChunkMeshState.READY:
                //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIDE_LENGTH, Color4.Green);
                //     break;
            }
            // if (CoordinateUtils.WorldToChunk(Camera.RenderingCamera.Position) == Position)
            //     DebugTextWindow.AddFrameText(Position + new Vector3(halfAChunk, halfAChunk, halfAChunk), $"HighestRenderedBlockY = {HighestRenderedBlockY}");
        }
#endif
    }


    public void Load()
    {
        if (_generationState == ChunkGenerationState.GENERATING)
        {
            Logger.LogWarning("Multiple generation jobs enqueued for chunk!");
        }
        _generationState = ChunkGenerationState.GENERATING;
        _isLoaded = true;
        Interlocked.Increment(ref _currentJobId);
        GlobalThreadPool.DispatchJob(new GenerationJob(_currentJobId, this, _generateJobCallback), WorkItemPriority.Normal);
    }


    public void Unload()
    {
        _isLoaded = false;
    }


    private void OnGenerated()
    {
        _meshState = ChunkMeshState.NONE;
        _generationState = ChunkGenerationState.READY;
        if (_containsRenderedBlocks)
        {
            if (CanBeMeshed())
            {
                EnqueueForMeshing();
            }
            else
            {
                _meshState = ChunkMeshState.WAITING_FOR_NEIGHBOURS_TO_GENERATE;
            }
        }
        else
        {
            OnReady();
        }
    }


    private void OnMeshed()
    {
        _meshState = ChunkMeshState.READY;
        
        OnReady();
    }


    public void SetMeshDirty()
    {
        if (!_containsRenderedBlocks)
            return;
        
        if (CanBeMeshed())
        {
            EnqueueForMeshing();
        }
        else
        {
            _meshState = ChunkMeshState.WAITING_FOR_NEIGHBOURS_TO_GENERATE;
        }
    }


    private void OnReady()
    {
        _generationState = ChunkGenerationState.READY;
    }
    
    
    private bool CanBeMeshed()
    {
        if (_generationState != ChunkGenerationState.READY)
            return false;

        // Check if the chunk neighbours are loaded (required for meshing)
        return GameWorld.CurrentGameWorld.RegionManager.AreChunkNeighboursGenerated(Position, true); //TODO: False, to not generate world border faces
    }


    /// <summary>
    /// Checks if the chunk is completely surrounded by opaque blocks, to see if it needs to be rendered.
    /// </summary>
    /// <returns>If the chunk is completely surrounded by opaque blocks.</returns>
    private bool IsSurroundedByBlocks()
    {
        return false;
    }


    private void EnqueueForMeshing()
    {
        if (IsSurroundedByBlocks())
        {
            Logger.LogWarning("TODO: Chunk is surrounded by blocks, skipping meshing.");
        }

        if (_meshState == ChunkMeshState.MESHING)
        {
            Logger.LogWarning("Multiple meshing jobs enqueued for chunk!");
        }
        _meshState = ChunkMeshState.MESHING;
        Interlocked.Increment(ref _currentJobId);
        GlobalThreadPool.DispatchJob(new MeshingJob(_currentJobId, this, _meshJobCallback), WorkItemPriority.High);
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
    public bool SetBlockState(Vector3i position, BlockState block, out BlockState oldBlock)
    {
        lock (_blockStorageLock)
        {
            _blockStorage.SetBlock(position.X, position.Y, position.Z, block, out oldBlock);
        
            // If the chunk has been meshed and a rendered block was changed, mark the chunk mesh as dirty.
            bool shouldDirtyMesh = _generationState == ChunkGenerationState.READY && _meshState != ChunkMeshState.MESHING && (oldBlock.IsRendered || block.IsRendered);
            
            if (shouldDirtyMesh)
                EnqueueForMeshing();
        
            _containsRenderedBlocks = _blockStorage.RenderedBlockCount > 0;
        
            return shouldDirtyMesh;
        }
    }


    /// <summary>
    /// Indexes to the block at the given position.
    /// If looping through a lot of blocks, make sure to iterate in z,y,x order to preserve cache locality:
    /// for z in range:
    ///    for y in range:
    ///       for x in range:
    ///          block = BlockMap[x, y, z]
    /// </summary>
    public BlockState GetBlockState(Vector3i position)
    {
        lock (_blockStorageLock)
        {
            return _blockStorage.GetBlock(position.X, position.Y, position.Z);
        }
    }
}