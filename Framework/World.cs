using BlockEngine.Framework.ECS.Entities;
using BlockEngine.Utils;

namespace BlockEngine.Framework;

public class World
{
    private readonly string _name;
    private readonly EntityManager _entityManager;


    public World(string name)
    {
        _name = name;
        _entityManager = new EntityManager();
        
        Logger.Log($"Loaded world '{_name}'");
    }
    
    
    public void Tick(double time)
    {
        _entityManager.Update(time);
    }


    public override string ToString()
    {
        return $"World '{_name}'";
    }
}