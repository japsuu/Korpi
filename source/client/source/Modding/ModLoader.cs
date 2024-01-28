using System.Collections;
using Korpi.Client.Blocks;
using Korpi.Client.Configuration;
using Korpi.Client.Logging;
using Korpi.Client.Modding.Blocks;
using Korpi.Client.Registries;
using Korpi.Client.Utils;

namespace Korpi.Client.Modding;

public static class ModLoader
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ModLoader));

    public static void LoadAllMods()
    {
        Logger.Info("Loading mods...");
        List<YamlMod> mods = new();
        List<YamlMod> builtInMods = new();

        IEnumerable fileEnumerable = Directory.EnumerateFiles(Constants.MODS_PATH, $"*.{Constants.YAML_MOD_FILE_EXTENSION}", SearchOption.AllDirectories);
        
        foreach (string filePath in fileEnumerable)
        {
            string fileContents = File.ReadAllText(filePath);
            YamlMod mod = YamlSerializationHelper.Deserialize<YamlMod>(fileContents);
            
            Logger.Info($"Discovered mod '{mod.Name}' by '{mod.Author}'.");

            if (mod.Name == null)
            {
                Logger.Warn($"Mod by '{mod.Author}' has no name, skipping.");
                continue;
            }
            
            if (mod.Author == null)
            {
                Logger.Warn($"Mod '{mod.Name}' has no author, skipping.");
                continue;
            }
            
            if (mod.Namespace == null)
            {
                Logger.Warn($"Mod '{mod.Name}' by '{mod.Author}' has no namespace, skipping.");
                continue;
            }
            
            mod.Namespace = mod.Namespace.ToLower();
            mod.SetContainingFolderPath(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("What?"));

            if (mod.IsBuiltin)
            {
                builtInMods.Add(mod);
                continue;
            }
            mods.Add(mod);
        }

        if (builtInMods.Count < 1)
        {
            Logger.Warn("No built-in mods found, creating default.");
            builtInMods.Add(GetBuiltinMods());
        }
        
        foreach (YamlMod mod in builtInMods)
        {
            BlockRegistry.RegisterBlocks(mod);
        }
        
        foreach (YamlMod mod in mods)
        {
            BlockRegistry.RegisterBlocks(mod);
        }
        
        Logger.Info("Finished loading mods.");
    }
    
    
    private static string GetDefaultBuiltinMod()
    {
        throw new NotImplementedException("TODO: Implement default builtin mod.");
        List<YamlBlockData> blocks = new()
        {
            new YamlBlockData("TestBlock", BlockRenderType.Opaque, "TestBlock")
        };

        YamlMod mod = new("Builtins", Constants.BUILT_IN_MOD_NAMESPACE, "Built-in blocks", "Japsu", blocks.ToArray());
        
        return YamlSerializationHelper.Serialize(mod);
    }


    private static YamlMod GetBuiltinMods()
    {
        string filePath = IoUtils.GetBuiltinModPath();
        
        if (!File.Exists(filePath))
        {
            string def = GetDefaultBuiltinMod();
            File.WriteAllText(filePath, def);
            Logger.Info("Created default builtin mod.");
        }
        
        string fileContents = File.ReadAllText(filePath);
        YamlMod mod = YamlSerializationHelper.Deserialize<YamlMod>(fileContents);
        mod.SetContainingFolderPath(IoUtils.GetBuiltinModFolderPath());
        return mod;
    }
}