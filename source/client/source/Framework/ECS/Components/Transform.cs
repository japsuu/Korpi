using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.ECS.Components;

public class Transform : Component
{
    public Transform? Parent { get; private set; }

    public Vector3 WorldPosition
    {
        get
        {
            if (Parent != null)
                return Parent.WorldPosition + LocalPosition;
            return LocalPosition;
        }
        set
        {
            if (Parent != null)
                LocalPosition = value - Parent.WorldPosition;
            else
                LocalPosition = value;
        }
    }
    public Vector3 WorldRotation
    {
        get
        {
            if (Parent != null)
                return Parent.WorldRotation + LocalRotation;
            return LocalRotation;
        }
        set
        {
            if (Parent != null)
                LocalRotation = value - Parent.WorldRotation;
            else
                LocalRotation = value;
        }
    }
    
    public Vector3 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 Scale;


    public Transform()
    {
        LocalPosition = Vector3.Zero;
        LocalRotation = Vector3.Zero;
        Scale = Vector3.One;
    }
    
    
    public void SetParent(Transform? parent, bool keepWorldPosition = true)
    {
        if (parent == null)
        {
            if (Parent != null)
            {
                if (keepWorldPosition)
                {
                    LocalPosition = WorldPosition;
                    LocalRotation = WorldRotation;
                }
                Parent = null;
            }
        }
        else
        {
            if (Parent != null)
            {
                if (keepWorldPosition)
                {
                    LocalPosition = WorldPosition - parent.WorldPosition;
                    LocalRotation = WorldRotation - parent.WorldRotation;
                }
            }
            Parent = parent;
        }
    }
    
    
    public Matrix4 GetWorldModelMatrix()
    {
        return
            Matrix4.CreateScale(Scale) *
            Matrix4.CreateRotationX(WorldRotation.X) *
            Matrix4.CreateRotationY(WorldRotation.Y) *
            Matrix4.CreateRotationZ(WorldRotation.Z) *
            Matrix4.CreateTranslation(WorldPosition);
    }
    
    
    public Matrix4 GetLocalModelMatrix()
    {
        return
            Matrix4.CreateScale(Scale) *
            Matrix4.CreateRotationX(LocalRotation.X) *
            Matrix4.CreateRotationY(LocalRotation.Y) *
            Matrix4.CreateRotationZ(LocalRotation.Z) *
            Matrix4.CreateTranslation(LocalPosition);
    }
    
    
    public void SetPosition(Vector3 position)
    {
        LocalPosition = position;
    }
    
    
    public void Translate(Vector3 translation)
    {
        LocalPosition += translation;
    }
    
    
    public void Rotate(Vector3 rotation)
    {
        LocalRotation += rotation;
    }
    
    
    public void ScaleBy(Vector3 scale)
    {
        Scale += scale;
    }


    public override string ToString()
    {
        return $"Transform: WorldPosition={WorldPosition} (Local={LocalPosition}), WorldRotation={WorldRotation} (Local={LocalRotation}), Scale={Scale}";
    }
}