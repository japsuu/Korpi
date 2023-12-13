using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BlockEngine.Utils;

public static class YamlHelper
{
    public static T Deserialize<T>(string yaml)
    {
        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<T>(yaml);
    }
    
    public static string Serialize<T>(T obj)
    {
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return serializer.Serialize(obj);
    }
}