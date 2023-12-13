namespace BlockEngine.Client.Framework.Blocks.Serialization;

public struct BlockFaceTextureCollectionData
{
    public BlockFaceTextureData Front;
    public BlockFaceTextureData Back;
    public BlockFaceTextureData Left;
    public BlockFaceTextureData Right;
    public BlockFaceTextureData Top;
    public BlockFaceTextureData Bottom;


    public BlockFaceTextureCollectionData(BlockFaceTextureData front, BlockFaceTextureData back, BlockFaceTextureData left, BlockFaceTextureData right, BlockFaceTextureData top, BlockFaceTextureData bottom)
    {
        Front = front;
        Back = back;
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }


    public BlockFaceTextureCollectionData(IReadOnlyList<BlockFaceTextureData> textures)
    {
        Front = textures[0];
        Back = textures[1];
        Left = textures[2];
        Right = textures[3];
        Top = textures[4];
        Bottom = textures[5];
    }


    public BlockFaceTextureCollectionData(string texture)
    {
        Front = new BlockFaceTextureData(texture);
        Back = new BlockFaceTextureData(texture);
        Left = new BlockFaceTextureData(texture);
        Right = new BlockFaceTextureData(texture);
        Top = new BlockFaceTextureData(texture);
        Bottom = new BlockFaceTextureData(texture);
    }
}