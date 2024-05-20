using KorpiEngine.Rendering.Cameras;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Cameras;

/// <summary>
/// A static camera used for photo mode.
/// Does not have any movement/rotation capabilities.
/// </summary>
public class PhotoModeCamera : Camera
{
    public static PhotoModeCamera Instance { get; private set; } = null!;
    
    
    public static void Create(Vector3 localPosition, float pitch, float yaw)
    {
        if (Instance != null)
            throw new Exception("Only one photo mode camera can be loaded at a time!");
        
        Instance = new PhotoModeCamera(localPosition, pitch, yaw);
    }
    
    
    private PhotoModeCamera(Vector3 localPosition, float pitch, float yaw) : base(localPosition, pitch, yaw, -100)
    {
        
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