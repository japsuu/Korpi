﻿using Korpi.Client.Blocks;
using Korpi.Client.Debugging;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.Generation.TerrainGenerators;
using Korpi.Client.Physics;
using Korpi.Client.Player;
using Korpi.Client.Registries;
using Korpi.Client.World.Chunks;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Logging;
using KorpiEngine.Rendering.Cameras;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Cursor = KorpiEngine.Core.InputManagement.Cursor;

namespace Korpi.Client.World;

public class GameWorld
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GameWorld));
    
    public static GameWorld CurrentGameWorld { get; private set; } = null!;

    public static event Action<WorldEvent>? WorldEventPublished;
    
    public readonly ChunkManager ChunkManager;      // TODO: Make private, or wrap around a function
    public readonly ITerrainGenerator TerrainGenerator;

    private readonly string _name;


    public GameWorld(string name)
    {
        _name = name;
        ChunkManager = new ChunkManager();
        TerrainGenerator = Simplex3DTerrainGenerator.Default();
        
        if (CurrentGameWorld != null)
            throw new Exception("For now, only one world can be loaded at a time");
        CurrentGameWorld = this;
        
        Logger.Info($"Loaded world '{_name}'");
    }
    
    
    public void Update()
    {
        DebugStats.LastRaycastResult = RaycastWorld(Camera.RenderingCamera.Position, Camera.RenderingCamera.Forward, 10);
    }
    
    
    public void FixedUpdate()
    {
        ChunkManager.Tick();
        DebugStats.LoadedChunkCount = ChunkManager.LoadedColumnsCount;
    }


    public BlockState RaycastWorld(Vector3 start, Vector3 direction, float maxDistance)
    {
        Ray ray = new Ray(start, direction);
        RaycastResult raycastResult = ChunkManager.RaycastBlocks(ray, maxDistance);

        if (Cursor.IsGrabbed)
        {
            if (Input.MouseState.IsButtonPressed(MouseButton.Left))
            {
                if (raycastResult.Hit)
                {
                    ChunkManager.SetBlockStateAtWorld(raycastResult.HitBlockPosition, BlockRegistry.Air.GetDefaultState());
                }
            }
            else if (Input.MouseState.IsButtonPressed(MouseButton.Right))
            {
                if (raycastResult.Hit)
                {
                    ChunkManager.SetBlockStateAtWorld(
                        raycastResult.HitBlockPosition + raycastResult.HitBlockFace.Normal(),
                        BlockRegistry.GetBlock(PlayerEntity.SelectedBlockType).GetDefaultState());
                }
            }
        }

#if DEBUG
        if (Configuration.ClientConfig.Rendering.Debug.RenderRaycastHit)
            DebugDrawer.DrawSphere(raycastResult.HitPosition, 0.5f, Color4.Red);
        
        if (Configuration.ClientConfig.Rendering.Debug.HighlightTargetedBlock)
#endif
            if (!raycastResult.BlockState.IsAir)
                DebugDrawer.DrawBox(raycastResult.HitBlockPosition + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 1, 1), Color4.Red);
        
        return raycastResult.BlockState;
    }


    public override string ToString()
    {
        return $"World '{_name}'";
    }


    public static void ReloadAllChunks()
    {
        PublishWorldEvent(WorldEvent.RELOAD_ALL_CHUNKS);
    }


    public static void RegenerateAllChunks()
    {
        PublishWorldEvent(WorldEvent.REGENERATE_ALL_CHUNKS);
    }
    
    
    private static void PublishWorldEvent(WorldEvent worldEvent)
    {
        WorldEventPublished?.Invoke(worldEvent);
    }
}