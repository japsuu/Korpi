using BlockEngine.Framework.Chunks;
using BlockEngine.Framework.ECS.Entities;
using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework;

public class World
{
    private readonly string _name;
    private readonly ChunkManager _chunkManager;
    private readonly EntityManager _entityManager;


    public World(string name)
    {
        _name = name;
        _chunkManager = new ChunkManager();
        _entityManager = new EntityManager();
        
        Logger.Log($"Loaded world '{_name}'");
    }
    
    
    public void Tick(Vector3 cameraPos, double time)
    {
        _chunkManager.Tick(cameraPos, time);
        _entityManager.Update(time);
    }
    
    
    public void DrawChunks(Vector3 cameraPos, Shader chunkShader)
    {
        _chunkManager.Draw(cameraPos, chunkShader);
    }


    public override string ToString()
    {
        return $"World '{_name}'";
    }
}