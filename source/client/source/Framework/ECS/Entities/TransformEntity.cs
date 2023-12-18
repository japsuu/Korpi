﻿using BlockEngine.Client.Framework.ECS.Components;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.ECS.Entities;

/// <summary>
/// Basic entity with a transform component.
/// </summary>
public abstract class TransformEntity : Entity
{
    public readonly TransformComponent Transform;


    protected TransformEntity(Vector3 localPosition = default)
    {
        Transform = AddComponent<TransformComponent>();
        
        Transform.LocalPosition = localPosition;
    }
}