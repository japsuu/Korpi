namespace BlockEngine.Framework.Blocks.Serialization;

/// <summary>
/// Serializable data of a block.
/// Loaded from YAML files.
/// </summary>
public sealed class YamlBlockData
{
    public string? Name;
    
    public BlockRenderType RenderType;
    
    public BlockFaceTextureCollectionData FaceTextures;


    public YamlBlockData()
    {
    }


    public YamlBlockData(string? name, BlockRenderType renderType, BlockFaceTextureCollectionData faceTextures)
    {
        Name = name;
        RenderType = renderType;
        FaceTextures = faceTextures;
    }


    public YamlBlockData(string? name, BlockRenderType renderType, IReadOnlyList<BlockFaceTextureData> faceTextures)
    {
        Name = name;
        RenderType = renderType;
        FaceTextures = new BlockFaceTextureCollectionData(faceTextures);
    }


    public YamlBlockData(string? name, BlockRenderType renderType, string faceTexture)
    {
        Name = name;
        RenderType = renderType;
        FaceTextures = new BlockFaceTextureCollectionData(faceTexture);
    }
    
    
    public static YamlBlockData Empty(string? name) => new(name, BlockRenderType.None, new BlockFaceTextureCollectionData());
}