namespace BlockEngine.Client.Framework.Blocks.Serialization;

public struct BlockFaceTextureData
{
    public string[] Textures;


    public BlockFaceTextureData(string[] textures)
    {
        Textures = textures;
    }


    public BlockFaceTextureData(string texture)
    {
        Textures = new []{ texture };
    }
}