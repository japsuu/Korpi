using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

public enum BlockFace
{
    XPositive = 0,
    YPositive = 1,
    ZPositive = 2,
    XNegative = 3,
    YNegative = 4,
    ZNegative = 5
}


public static class BlockFaceExtensions
{
    public static Vector3i Normal(this BlockFace face)
    {
        return face switch
        {
            BlockFace.XPositive => new Vector3i(1, 0, 0),
            BlockFace.YPositive => new Vector3i(0, 1, 0),
            BlockFace.ZPositive => new Vector3i(0, 0, 1),
            BlockFace.XNegative => new Vector3i(-1, 0, 0),
            BlockFace.YNegative => new Vector3i(0, -1, 0),
            BlockFace.ZNegative => new Vector3i(0, 0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }
}