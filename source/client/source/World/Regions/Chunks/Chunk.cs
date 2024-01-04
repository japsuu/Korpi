using BlockEngine.Client.Debugging.Drawing;
using BlockEngine.Client.Logging;
using BlockEngine.Client.World.Regions.Chunks.Blocks;
using BlockEngine.Client.World.Regions.Chunks.BlockStorage;
using OpenTK.Mathematics;

namespace BlockEngine.Client.World.Regions.Chunks;

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
        GENERATING = 0,
        
        /// <summary>
        /// The chunk has been generated.
        /// </summary>
        READY = 1
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

    private bool _containsRenderedBlocks;
    private bool _isLoaded;
    
    public readonly Vector3i Position;
    public readonly int Top;
    public readonly int Bottom;

    private ChunkGenerationState _generationState;
    private ChunkMeshState _meshState;
    
    public bool IsGenerated => _generationState == ChunkGenerationState.READY;
    public bool ShouldBeRendered => IsGenerated && _meshState >= ChunkMeshState.MESHING;


    public Chunk(Vector3i position)
    {
        Position = position;
        Top = position.Y + Constants.CHUNK_SIZE - 1;
        Bottom = position.Y;
        _containsRenderedBlocks = false;
        _generationState = ChunkGenerationState.GENERATING;
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
            const float halfAChunk = Constants.CHUNK_SIZE / 2f;
            Vector3 centerOffset = new Vector3(halfAChunk, 0, halfAChunk);
            // if (_containsRenderedBlocks)
            // {
            //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIZE, Color4.Green);
            // }
            // else
            // {
            //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIZE, Color4.Red);
            // }
            // return;
            switch (_meshState)
            {
                case ChunkMeshState.NONE:
                    DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIZE, Color4.Red);
                    break;
                case ChunkMeshState.WAITING_FOR_NEIGHBOURS_TO_GENERATE:
                    DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIZE, Color4.Blue);
                    break;
                case ChunkMeshState.MESHING:
                    DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIZE, Color4.Yellow);
                    break;
                // case ChunkMeshState.READY:
                //     DebugDrawer.DrawLine(Position + centerOffset, Position + centerOffset + Vector3i.UnitY * Constants.CHUNK_SIZE, Color4.Green);
                //     break;
            }
            // if (CoordinateUtils.WorldToChunk(Camera.RenderingCamera.Position) == Position)
            //     DebugTextWindow.AddFrameText(Position + new Vector3(halfAChunk, halfAChunk, halfAChunk), $"HighestRenderedBlockY = {HighestRenderedBlockY}");
        }
#endif
    }


    public void Load()
    {
        _isLoaded = true;
        GameWorld.CurrentGameWorld.ChunkGenerator.Enqueue(Position);
    }


    public void Unload()
    {
        _isLoaded = false;
    }


    public void OnGenerated()
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
    
    
    public void OnMeshed()
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
            throw new InvalidOperationException("Tried to mesh a chunk that is not generated!");

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
        _meshState = ChunkMeshState.MESHING;
        GameWorld.CurrentGameWorld.ChunkMesher.Enqueue(Position);
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