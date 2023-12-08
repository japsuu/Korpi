using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Chunks;
using BlockEngine.Framework.Debugging;
using BlockEngine.Framework.ECS.Entities;
using BlockEngine.Framework.Physics;
using BlockEngine.Framework.Rendering;
using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework;

public class World
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
    
    
    public void Tick(Camera camera, double time)
    {
        ChunkManager.Tick(camera.Transform.Position, time);
        _entityManager.Update(time);
        CameraStats.RaycastResult = RaycastWorld(camera.Transform.Position, camera.Front, 10);
    }
    
    
    public void DrawChunks(Vector3 cameraPos, Shader chunkShader)
    {
        ChunkManager.Draw(cameraPos, chunkShader);
    }
    
    
    public BlockState RaycastWorld(Vector3 start, Vector3 direction, float maxDistance)
    {
        Ray ray = new Ray(start, direction);
        RaycastResult raycastResult = ChunkManager.RaycastBlocks(ray, maxDistance);
        
        if (DebugSettings.RenderRaycastHit)
            DebugDrawer.DrawSphere(raycastResult.HitPosition, 0.5f, Color4.Red);
        
        if (DebugSettings.RenderRaycastHitBlock && !raycastResult.BlockState.IsAir)
            DebugDrawer.DrawBox(raycastResult.HitBlockPosition + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 1, 1), Color4.Red);
        
        return raycastResult.BlockState;
    }


    public override string ToString()
    {
        return $"World '{_name}'";
    }
}