namespace BlockEngine.Framework.Registries;

/// <summary>
/// Allows retrieving references to values through either a namespaced key or an ID.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NamespacedIdRegistry<T> where T : IHasId
{
    private readonly string _name;
    private readonly Dictionary<string, T> _keyToValue;
    private readonly Dictionary<ushort, T> _idToValue;


    public NamespacedIdRegistry(string name)
    {
        _name = name;
        _keyToValue = new Dictionary<string, T>();
        _idToValue = new Dictionary<ushort, T>();
    }
    
    
    public T Register(string name, T value)
    {
        ushort index = (ushort)_keyToValue.Count;
        _keyToValue.Add(name, value);
        _idToValue.Add(index, value);
        value.AssignId(index);
        return value;
    }
    
    
    public T GetValue(ushort id)
    {
        return _idToValue[id];
    }
    
    
    public T GetValue(string key)
    {
        return _keyToValue[key];
    }


    public override string ToString()
    {
        return $"Registry: {_name}";
    }
}