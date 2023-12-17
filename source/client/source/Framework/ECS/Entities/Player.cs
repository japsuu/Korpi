using BlockEngine.Client.Framework.ECS.Components;
using BlockEngine.Client.Framework.Rendering.Cameras;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Client.Framework.ECS.Entities;

/// <summary>
/// Caveman'd player entity. Follows the camera around.
/// </summary>
public class Player : TransformEntity
{
    private static readonly Vector3 CameraOffset = new(0, 1.5f, 0);
    
    public static Player LocalPlayer { get; private set; } = null!;

    public bool IsCameraLocked { get; private set; }
    public readonly Camera Camera;
    

    public Player(Vector3 position, float cameraYaw, float cameraPitch, float cameraAspectRatio)
    {
        if (LocalPlayer != null)
            throw new Exception("For now, only one player can be loaded at a time");
        LocalPlayer = this;
        
        Camera = new Camera(position + CameraOffset, cameraYaw, cameraPitch, cameraAspectRatio);
        
        Transform.WorldPosition = position;
        LockCamera();
        
        AddComponent(new PlayerRenderer(this));
    }
    
    
    public void FreeCamera()
    {
        Transform.SetParent(null);
        IsCameraLocked = false;
    }
    
    
    public void LockCamera()
    {
        Camera.Transform.WorldPosition = Transform.WorldPosition + CameraOffset;
        Transform.SetParent(Camera.Transform);
        IsCameraLocked = true;
    }


    protected override void OnUpdate(double time)
    {
        if (Input.KeyboardState.IsKeyPressed(Keys.F5))
        {
            if (IsCameraLocked)
                FreeCamera();
            else
                LockCamera();
        }
    }
}