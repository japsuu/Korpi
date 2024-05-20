using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Korpi.Client.Modding;

public static class YamlSerializationHelper
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