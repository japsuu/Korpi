using System.Diagnostics;
using Common.Logging;
using Korpi.Client.Blocks;
using Korpi.Client.Blocks.Textures;
using Korpi.Client.Configuration;
using Korpi.Client.Exceptions;
using Korpi.Client.Modding;
using Korpi.Client.Modding.Blocks;
using Korpi.Client.Utils;

namespace Korpi.Client.Registries;

/// <summary>
/// Allows retrieving block references through either a namespaced name or an ID.
/// </summary>
public static class BlockRegistry
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(BlockRegistry));

    private static readonly List<Block> Values = new();
    private static readonly List<BlockState> DefaultStateValues = new();
    private static readonly Dictionary<string, Block> NameToValue = new();
    private static ushort nextId;
    
    public static readonly Block Air = RegisterNewBuiltinBlock(Constants.BUILT_IN_MOD_NAMESPACE, YamlBlockData.Empty("Air"));


    public static int GetBlockCount() => Values.Count;
    
    
    public static void RegisterBlocks(YamlMod mod)
    {
        if (mod.ContainingFolderPath == null)
        {
            Logger.Warn($"Mod {mod.Name} by {mod.Author} has no containing folder path, cannot register blocks for it.");
            return;
        }
        
        if (mod.Blocks == null)
            return;
        
        if (mod.Namespace == null)
        {
            Logger.Warn($"Mod {mod.Name} by {mod.Author} has no namespace, cannot register blocks for it.");
            return;
        }
        
        foreach (YamlBlockData blockCreationData in mod.Blocks)
        {
            RegisterNewBlock(mod.Namespace, mod.ContainingFolderPath, blockCreationData);
        }

        Logger.Info($"Registered {mod.Blocks.Length} blocks from mod '{mod.Name}' by {mod.Author}.");
    }
    
    
    public static Block GetBlock(ushort id)
    {
        Debug.Assert(id < Values.Count, $"Block with ID {id} is not registered.");
        return Values[id];
    }
    
    
    public static BlockState GetBlockDefaultState(ushort id)
    {
        Debug.Assert(id < DefaultStateValues.Count, $"Block with ID {id} is not registered.");
        return DefaultStateValues[id];
    }
    
    
    public static Block GetBlock(string namespacedName)
    {
        Debug.Assert(NameToValue.ContainsKey(namespacedName), $"Block with name {namespacedName} is not registered.");
        return NameToValue[namespacedName];
    }
    
    
    public static Block GetBlock(string nameSpace, string name)
    {
        string fullName = nameSpace + ":" + name;
        return NameToValue[fullName];
    }


    private static Block RegisterNewBuiltinBlock(string nameSpace, YamlBlockData data)
    {
        return RegisterNewBlock(nameSpace, IoUtils.GetBuiltinModFolderPath(), data);
    }


    private static Block RegisterNewBlock(string nameSpace, string folderPath, YamlBlockData data)
    {
        string namespacedName = nameSpace + ":" + data.Name!.ToLower();
        // Replace spaces with underscores
        namespacedName = namespacedName.Replace(' ', '_');
        
        if (NameToValue.ContainsKey(namespacedName))
            throw new IdClashException($"Block with name {namespacedName} is already registered.");
        
        if (nextId == ushort.MaxValue)
            throw new IdOverflowException("Block ID overflow. Too many blocks registered.");
        
        BlockFaceTextureCollection? textures = data.RenderType == BlockRenderType.None ?
            null :
            TextureRegistry.RegisterBlockTextures(folderPath, data.TextureData);
        
        Block block = new(nextId, data.Name!.ToLower(), data.RenderType, textures);
        
        Values.Add(block);
        DefaultStateValues.Add(block.GetDefaultState());
        NameToValue.Add(namespacedName, block);
        
        nextId++;
        return block;
    }
}