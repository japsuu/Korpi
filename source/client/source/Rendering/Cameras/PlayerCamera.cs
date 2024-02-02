using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Cameras;

public class PlayerCamera : Camera
{
    private const float LOOK_SENSITIVITY = 0.2f;
    
    
    public PlayerCamera(Vector3 localPosition, float pitch, float yaw) : base(localPosition, pitch, yaw)
    {
    }


    public void UpdateRotation()
    {
        // TODO: Project the Front/Right vectors onto the XZ plane while keeping their magnitude and use those for movement.
        // TODO: Use world up/down instead of camera up/down for vertical movement.
        // TODO: Possibly use the Transform.forward property.

        // Calculate the offset of the mouse position
        float deltaX = Input.MouseState.X - Input.MouseState.PreviousX;
        float deltaY = Input.MouseState.Y - Input.MouseState.PreviousY;

        YawDegrees += deltaX * LOOK_SENSITIVITY;
        PitchDegrees -= deltaY * LOOK_SENSITIVITY; // Reversed since y-coordinates range from bottom to top
    }

    
    protected override Vector3 CalculateForwardVector(float pitch, float yaw)
    {
        // First, the front matrix is calculated using some basic trigonometry.
        float x = MathF.Cos(pitch) * MathF.Cos(yaw);
        float y = MathF.Sin(pitch);
        float z = MathF.Cos(pitch) * MathF.Sin(yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        return Vector3.Normalize(new Vector3(x, y, z));
    }


    protected override Vector3 CalculateRightVector(Vector3 forward)
    {
        // Calculate both the right and the up vector using cross product.
        // We are calculating the right from the "global" up.
        return Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
    }


    protected override Vector3 CalculateUpVector(Vector3 right, Vector3 forward)
    {
        return Vector3.Normalize(Vector3.Cross(right, forward));
    }
}