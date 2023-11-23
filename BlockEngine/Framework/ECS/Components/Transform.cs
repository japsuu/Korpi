using OpenTK.Mathematics;

namespace BlockEngine.Framework.ECS.Components;

public class Transform : Component
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;
    
    
    public Matrix4 GetModelMatrix()
    {
        return
            Matrix4.CreateScale(Scale) *
            Matrix4.CreateRotationX(Rotation.X) *
            Matrix4.CreateRotationY(Rotation.Y) *
            Matrix4.CreateRotationZ(Rotation.Z) *
            Matrix4.CreateTranslation(Position);
    }
    
    
    public void Translate(Vector3 translation)
    {
        Position += translation;
    }
    
    
    public void Rotate(Vector3 rotation)
    {
        Rotation += rotation;
    }
    
    
    public void ScaleBy(Vector3 scale)
    {
        Scale += scale;
    }
    
    
    public override string ToString()
    {
        return $"Transform: Position={Position}, Rotation={Rotation}, Scale={Scale}";
    }
}