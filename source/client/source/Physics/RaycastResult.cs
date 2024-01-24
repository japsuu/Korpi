using Korpi.Client.World.Chunks.Blocks;
using OpenTK.Mathematics;

namespace Korpi.Client.Physics;

public struct RaycastResult
{
    public readonly bool Hit;
    public readonly Vector3 HitPosition;
    public readonly Vector3i HitBlockPosition;
    public readonly BlockFace HitBlockFace;
    public readonly BlockState BlockState;


    public RaycastResult(bool hit, Vector3 hitPosition, Vector3i hitBlockPosition, BlockFace hitBlockFace, BlockState blockState)
    {
        Hit = hit;
        HitPosition = hitPosition;
        HitBlockPosition = hitBlockPosition;
        HitBlockFace = hitBlockFace;
        BlockState = blockState;
    }
}