namespace BlockEngine.Client.Modding.Blocks;

public struct YamlBlockFaceTextureData
{
    public string[] Textures;


    public YamlBlockFaceTextureData(string[] textures)
    {
        Textures = textures;
    }


    public YamlBlockFaceTextureData(string texture)
    {
        Textures = new []{ texture };
    }
}