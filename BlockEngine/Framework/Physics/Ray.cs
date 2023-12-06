using OpenTK.Mathematics;

namespace BlockEngine.Framework.Physics;

public class Ray
{
    public readonly Vector3 Start;
    public readonly Vector3 Direction;


    public Ray(Vector3 start, Vector3 direction)
    {
        Start = start;
        Direction = direction;
    }
}