using Korpi.Client.Configuration;
using Korpi.Client.Debugging;
using Korpi.Client.Debugging.Drawing;
using Korpi.Client.ECS.Entities;
using Korpi.Client.Generation.TerrainGenerators;
using Korpi.Client.Logging;
using Korpi.Client.Physics;
using Korpi.Client.Registries;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Window;
using Korpi.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Korpi.Client.World;

public class GameWorld
{
    public static GameWorld CurrentGameWorld { get; private set; } = null!;
    
    public readonly RegionManager RegionManager;      // TODO: Make private, and wrap around a function
    public readonly ITerrainGenerator TerrainGenerator;

    private readonly string _name;
    private readonly EntityManager _entityManager;


    public GameWorld(string name)
    {
        _name = name;
        RegionManager = new RegionManager();
        TerrainGenerator = PerlinTerrainGenerator.Default();
        _entityManager = new EntityManager();
        
        if (CurrentGameWorld != null)
            throw new Exception("For now, only one world can be loaded at a time");
        CurrentGameWorld = this;
        
        Logger.Log($"Loaded world '{_name}'");
    }
    
    
    public void Update()
    {
        _entityManager.Update();
        CameraWindowData.LastRaycastResult = RaycastWorld(Camera.RenderingCamera.Position, Camera.RenderingCamera.Forward, 10);
    }
    
    
    public void FixedUpdate()
    {
        RegionManager.Tick();
        _entityManager.FixedUpdate();
        DebugStats.LoadedRegionCount = RegionManager.LoadedRegionsCount;
    }
    
    
    public void Draw()
    {
        RegionManager.Draw(ShaderManager.ChunkShader);
        _entityManager.Draw();
    }
    
    
    public BlockState RaycastWorld(Vector3 start, Vector3 direction, float maxDistance)
    {
        Ray ray = new Ray(start, direction);
        RaycastResult raycastResult = RegionManager.RaycastBlocks(ray, maxDistance);

        if (!GameClient.IsPlayerInGui)
        {
            if (Input.MouseState.IsButtonPressed(MouseButton.Left))
            {
                if (raycastResult.Hit)
                {
                    RegionManager.SetBlockStateAtWorld(raycastResult.HitBlockPosition, BlockRegistry.Air.GetDefaultState());
                }
            }
            else if (Input.MouseState.IsButtonPressed(MouseButton.Right))
            {
                if (raycastResult.Hit)
                {
                    RegionManager.SetBlockStateAtWorld(
                        raycastResult.HitBlockPosition + raycastResult.HitBlockFace.Normal(),
                        BlockRegistry.GetBlock(PlayerEntity.SelectedBlockType).GetDefaultState());
                }
            }
        }

#if DEBUG
        if (ClientConfig.DebugModeConfig.RenderRaycastHit)
            DebugDrawer.DrawSphere(raycastResult.HitPosition, 0.5f, Color4.Red);
        
        if (ClientConfig.DebugModeConfig.RenderRaycastHitBlock && !raycastResult.BlockState.IsAir)
            DebugDrawer.DrawBox(raycastResult.HitBlockPosition + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 1, 1), Color4.Red);
#endif
        
        return raycastResult.BlockState;
    }


    public override string ToString()
    {
        return $"World '{_name}'";
    }
}