namespace BlockEngine.Client.Registries;

/// <summary>
/// Allows retrieving references to values through either a namespaced key or an ID.
/// </summary>
/// <typeparam name="T"></typeparam>
public class IndexedRegistry<T> where T : IHasId
{
    private readonly string _name;
    private readonly List<T> _values;


    public IndexedRegistry(string name)
    {
        _name = name;
        _values = new List<T>();
    }
    
    
    public T Register(T value)
    {
        ushort index = (ushort)_values.Count;
        _values.Add(value);
        value.AssignId(index);
        return value;
    }
    
    
    public T GetValue(ushort id)
    {
        return _values[id];
    }


    public override string ToString()
    {
        return $"Registry: {_name}";
    }
}