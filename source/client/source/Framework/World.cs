using BlockEngine.Client.Framework.Blocks;
using BlockEngine.Client.Framework.Chunks;
using BlockEngine.Client.Framework.Configuration;
using BlockEngine.Client.Framework.Debugging;
using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Meshing;
using BlockEngine.Client.Framework.Physics;
using BlockEngine.Client.Framework.Rendering.Shaders;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework;

public class World : IDisposable
{
    public static World CurrentWorld { get; private set; } = null!;
    
    public readonly ChunkManager ChunkManager;

    private readonly string _name;
    private readonly EntityManager _entityManager;


    public World(string name)
    {
        _name = name;
        ChunkManager = new ChunkManager();
        _entityManager = new EntityManager();
        
        if (CurrentWorld != null)
            throw new Exception("For now, only one world can be loaded at a time");
        CurrentWorld = this;
        
        Logger.Log($"Loaded world '{_name}'");
    }
    
    
    public void Tick()
    {
        ChunkManager.Tick();
        ChunkGenerator.ProcessQueues();
        ChunkMesher.ProcessQueues();
        RenderingStats.LoadedColumnCount = ChunkManager.LoadedColumnsCount;
        _entityManager.Update();
        CameraStats.RaycastResult = RaycastWorld(PlayerEntity.LocalPlayerEntity.ViewPosition, PlayerEntity.LocalPlayerEntity.ViewForward, 10);
    }
    
    
    public void Draw()
    {
        ChunkManager.Draw(PlayerEntity.LocalPlayerEntity.Transform.LocalPosition, ShaderManager.ChunkShader);
        _entityManager.Draw();
    }
    
    
    public BlockState RaycastWorld(Vector3 start, Vector3 direction, float maxDistance)
    {
        Ray ray = new Ray(start, direction);
        RaycastResult raycastResult = ChunkManager.RaycastBlocks(ray, maxDistance);

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
    }
}