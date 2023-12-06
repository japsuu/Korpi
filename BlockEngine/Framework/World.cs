using BlockEngine.Framework.Chunks;
using BlockEngine.Framework.ECS.Entities;
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
    
    
    public void Tick(Vector3 cameraPos, double time)
    {
        ChunkManager.Tick(cameraPos, time);
        _entityManager.Update(time);
    }
    
    
    public void DrawChunks(Vector3 cameraPos, Shader chunkShader)
    {
        ChunkManager.Draw(cameraPos, chunkShader);
    }


    public override string ToString()
    {
        return $"World '{_name}'";
    }
}