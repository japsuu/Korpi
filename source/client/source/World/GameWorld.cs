using BlockEngine.Client.Configuration;
using BlockEngine.Client.Debugging;
using BlockEngine.Client.Debugging.Drawing;
using BlockEngine.Client.ECS.Entities;
using BlockEngine.Client.Generation;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Meshing;
using BlockEngine.Client.Physics;
using BlockEngine.Client.Registries;
using BlockEngine.Client.Rendering.Cameras;
using BlockEngine.Client.Rendering.Shaders;
using BlockEngine.Client.Window;
using BlockEngine.Client.World.Chunks.Blocks;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Client.World;

public class GameWorld : IDisposable
{
    public static GameWorld CurrentGameWorld { get; private set; } = null!;
    
    public readonly ChunkManager ChunkManager;      // TODO: Make private, and wrap around a function
    public readonly ChunkGenerator ChunkGenerator;  // TODO: Make private, and wrap around a function
    public readonly ChunkMesher ChunkMesher;        // TODO: Make private, and wrap around a function

    private readonly string _name;
    private readonly EntityManager _entityManager;


    public GameWorld(string name)
    {
        _name = name;
        ChunkManager = new ChunkManager();
        ChunkGenerator = new ChunkGenerator();
        ChunkMesher = new ChunkMesher();
        _entityManager = new EntityManager();
        
        if (CurrentGameWorld != null)
            throw new Exception("For now, only one world can be loaded at a time");
        CurrentGameWorld = this;
        
        Logger.Log($"Loaded world '{_name}'");
    }
    
    
    public void Tick()
    {
        ChunkManager.Tick();
        ChunkGenerator.ProcessQueues();
        ChunkMesher.ProcessQueues();
        _entityManager.Update();
        DebugStats.LoadedColumnCount = ChunkManager.LoadedColumnsCount;
        CameraWindowData.LastRaycastResult = RaycastWorld(Camera.RenderingCamera.Position, Camera.RenderingCamera.Forward, 10);
    }
    
    
    public void Draw()
    {
        ChunkManager.Draw(ShaderManager.ChunkShader);
        _entityManager.Draw();
    }
    
    
    public BlockState RaycastWorld(Vector3 start, Vector3 direction, float maxDistance)
    {
        Ray ray = new Ray(start, direction);
        RaycastResult raycastResult = ChunkManager.RaycastBlocks(ray, maxDistance);

        if (Input.MouseState.IsButtonPressed(MouseButton.Left))
        {
            if (raycastResult.Hit)
            {
                ChunkManager.SetBlockStateAt(raycastResult.HitBlockPosition, BlockRegistry.Air.GetDefaultState());
            }
        }
        else if (Input.MouseState.IsButtonPressed(MouseButton.Right))
        {
            if (raycastResult.Hit)
            {
                ChunkManager.SetBlockStateAt(raycastResult.HitBlockPosition + raycastResult.HitBlockFace.Normal(), BlockRegistry.GetBlock(ClientConfig.DebugModeConfig.SelectedBlockType).GetDefaultState());
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
    
    
    public void Dispose()
    {
        ChunkGenerator.Dispose();
        ChunkMesher.Dispose();
    }
}