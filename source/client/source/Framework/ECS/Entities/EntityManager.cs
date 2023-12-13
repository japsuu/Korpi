namespace BlockEngine.Client.Framework.ECS.Entities;

public class EntityManager
{
    private readonly List<Entity> _entities = new();


    public EntityManager()
    {
        Entity.EnableEvent += OnEntityEnabledChanged;
    }


    public void Update(double time)
    {
        foreach (Entity obj in _entities)
        {
            obj.Update(time);
        }
    }


    private void OnEntityEnabledChanged(Entity obj, bool enabled)
    {
        if (enabled)
        {
            RegisterEntity(obj);
        }
        else
        {
            UnregisterEntity(obj);
        }
    }


    private void RegisterEntity(Entity obj)
    {
        _entities.Add(obj);
    }


    private void UnregisterEntity(Entity obj)
    {
        _entities.Remove(obj);
    }
}