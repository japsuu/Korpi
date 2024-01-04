using BlockEngine.Client.World.Regions.Chunks.Blocks;
using YamlDotNet.Serialization;

namespace BlockEngine.Client.Modding.Blocks;

/// <summary>
/// Serializable data of a block.
/// Loaded from YAML files.
/// </summary>
public sealed class YamlBlockData
{
    [YamlMember(Order = 0, Alias = "name")]
    public string? Name;
    
    [YamlMember(Order = 1, Alias = "render_type")]
    public BlockRenderType RenderType;
    
    [YamlMember(Order = 2, Alias = "face_textures")]
    public YamlBlockTextureData TextureData;


    public YamlBlockData()
    {
        
    }


    public YamlBlockData(string? name, BlockRenderType renderType, YamlBlockTextureData textureData)
    {
        Name = name;
        RenderType = renderType;
        TextureData = textureData;
    }


    public YamlBlockData(string? name, BlockRenderType renderType, IReadOnlyList<YamlBlockFaceTextureData> faceTextures)
    {
        Name = name;
        RenderType = renderType;
        TextureData = new YamlBlockTextureData(faceTextures);
    }


    public YamlBlockData(string? name, BlockRenderType renderType, string faceTexture)
    {
        Name = name;
        RenderType = renderType;
        TextureData = new YamlBlockTextureData(faceTexture);
    }
    
    
    public static YamlBlockData Empty(string? name) => new(name, BlockRenderType.None, new YamlBlockTextureData());
}