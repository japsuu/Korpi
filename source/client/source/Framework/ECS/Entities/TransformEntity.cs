using BlockEngine.Client.Framework.ECS.Components;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.ECS.Entities;

public abstract class TransformEntity : Entity
{
    public readonly Transform Transform;


    protected TransformEntity(Vector3 localPosition = default)
    {
        Transform = AddComponent<Transform>();
        
        Transform.LocalPosition = localPosition;
    }
}