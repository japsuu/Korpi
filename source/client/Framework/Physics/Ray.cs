using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Physics;

public class Ray
{
    public readonly Vector3 Start;
    public readonly Vector3 NormalizedDirection;


    public Ray(Vector3 start, Vector3 direction)
    {
        Start = start;
        NormalizedDirection = direction.Normalized();
    }


    public override string ToString()
    {
        return $"Ray {{ Start: {Start}, Direction: {NormalizedDirection} }}";
    }
}