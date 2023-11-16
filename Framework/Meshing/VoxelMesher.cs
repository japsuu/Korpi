using BlockEngine.Framework.Blocks;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;


public class Face
{
    public readonly FaceNormal Normal;
    public readonly Vector3 BlockPosition;
    public readonly ushort TextureIndex;

    public Face(FaceNormal normal, Vector3 blockPosition, ushort textureIndex)
    {
        Normal = normal;
        BlockPosition = blockPosition;
        TextureIndex = textureIndex;
    }
}

public enum FaceNormal
{
    XPos,
    XNeg,
    YNeg,
    YPos,
    ZPos,
    ZNeg,
}

public class VoxelMesher
{
    private void AddFace(/*List<VertexPositionNormalTexture> vertices, List<int> indices, Vector3 position, Vector3 normal, Vector2 texturePosition*/)
    {
        throw new NotImplementedException();
        /*int index = vertices.Count;

        vertices.Add(new VertexPositionNormalTexture(position, normal, texturePosition));
        vertices.Add(new VertexPositionNormalTexture(position, normal, texturePosition));
        vertices.Add(new VertexPositionNormalTexture(position, normal, texturePosition));
        vertices.Add(new VertexPositionNormalTexture(position, normal, texturePosition));

        indices.Add(index);
        indices.Add(index + 1);
        indices.Add(index + 2);
        indices.Add(index + 2);
        indices.Add(index + 1);
        indices.Add(index + 3);*/
    }
    
    
    private ushort GetBlockTextureIndex(Block block, FaceNormal normal)
    {
        throw new NotImplementedException();
    }
}