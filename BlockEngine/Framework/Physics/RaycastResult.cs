using BlockEngine.Framework.Blocks;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Physics;

public struct RaycastResult
{
    public readonly bool Hit;
    public readonly Vector3 HitPosition;
    public readonly Vector3i HitBlockPosition;
    public readonly BlockState BlockState;


    public RaycastResult(bool hit, Vector3 hitPosition, Vector3i hitBlockPosition, BlockState blockState)
    {
        Hit = hit;
        HitPosition = hitPosition;
        HitBlockPosition = hitBlockPosition;
        BlockState = blockState;
    }
}