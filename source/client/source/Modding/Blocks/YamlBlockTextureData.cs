namespace Korpi.Client.Modding.Blocks;

public struct YamlBlockTextureData
{
    public YamlBlockFaceTextureData Front;
    public YamlBlockFaceTextureData Back;
    public YamlBlockFaceTextureData Left;
    public YamlBlockFaceTextureData Right;
    public YamlBlockFaceTextureData Top;
    public YamlBlockFaceTextureData Bottom;


    public YamlBlockTextureData(YamlBlockFaceTextureData front, YamlBlockFaceTextureData back, YamlBlockFaceTextureData left, YamlBlockFaceTextureData right, YamlBlockFaceTextureData top, YamlBlockFaceTextureData bottom)
    {
        Front = front;
        Back = back;
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }


    public YamlBlockTextureData(IReadOnlyList<YamlBlockFaceTextureData> textures)
    {
        Front = textures[0];
        Back = textures[1];
        Left = textures[2];
        Right = textures[3];
        Top = textures[4];
        Bottom = textures[5];
    }


    public YamlBlockTextureData(string texture)
    {
        Front = new YamlBlockFaceTextureData(texture);
        Back = new YamlBlockFaceTextureData(texture);
        Left = new YamlBlockFaceTextureData(texture);
        Right = new YamlBlockFaceTextureData(texture);
        Top = new YamlBlockFaceTextureData(texture);
        Bottom = new YamlBlockFaceTextureData(texture);
    }
}