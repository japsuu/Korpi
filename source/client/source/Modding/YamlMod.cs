using BlockEngine.Client.Modding.Blocks;
using YamlDotNet.Serialization;

namespace BlockEngine.Client.Modding;

public sealed class YamlMod
{
    [YamlMember(Order = 0, Alias = "name")]
    public string? Name;
    
    [YamlMember(Order = 1, Alias = "description")]
    public string Description = "No description provided.";
    
    [YamlMember(Order = 2, Alias = "author")]
    public string? Author;
    
    [YamlMember(Order = 3, Alias = "namespace")]
    public string? Namespace;
    
    [YamlMember(Order = 4, Alias = "blocks")]
    public YamlBlockData[]? Blocks;
    
    // Non-YAML serialized fields.
    public bool IsBuiltin => Namespace == Constants.BUILT_INS_NAMESPACE;
    public string? ContainingFolderPath => _containingFolderPath;
    
    private string? _containingFolderPath;


    public YamlMod()
    {
    }


    public YamlMod(string? name, string? nameSpace, string? description, string? author, YamlBlockData[]? blocks)
    {
        Name = name;
        Namespace = nameSpace;
        if (description != null)
            Description = description;
        Author = author;
        Blocks = blocks;
    }
    
    
    public void SetContainingFolderPath(string containingFolderPath)
    {
        _containingFolderPath = containingFolderPath;
    }
}