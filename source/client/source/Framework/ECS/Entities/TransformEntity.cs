using BlockEngine.Client.Framework.ECS.Components;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.ECS.Entities;

public class TransformEntity : Entity
{
    public Transform Transform { get; private set; }
    
    
    public TransformEntity(Vector3 position = default)
    {
        Transform = AddComponent<Transform>();
        
        Transform.Position = position;
    }
}