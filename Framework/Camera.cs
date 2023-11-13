using BlockEngine.Framework.ECS.Entities;
using BlockEngine.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Framework;

public class Camera : TransformEntity
{
    private float _cameraFlySpeed = 1.5f;
    private const float SENSITIVITY = 0.2f;
    
    private Vector3 _front = -Vector3.UnitZ;
    
    // Rotation around the X axis (radians)
    private float _pitch;

    // Rotation around the Y axis (radians)
    private float _yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

    // The field of view of the camera (radians)
    private float _fov = MathHelper.PiOver2;
    private Vector2 _mouseLastPos;

    // This is simply the aspect ratio of the viewport, used for the projection matrix.
    public float AspectRatio { private get; set; }
    
    // We use this variable to keep track of whether this is the first time the mouse has moved.
    public bool IsMouseFirstMove = true;
    
    public bool IsInputEnabled = true;

    // Those vectors are directions pointing outwards from the camera to define how it rotated.
    public Vector3 Front => _front;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            // We clamp the pitch value between -89 and 89 to prevent gimbal lock and/or the camera from going upside down.
            float angle = MathHelper.Clamp(value, -89f, 89f);
            // We convert from degrees to radians as soon as the property is set to improve performance.
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            // We convert from degrees to radians as soon as the property is set to improve performance.
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    // The field of view (FOV) is the vertical angle of the camera view.
    public float Fov
    {
        get => MathHelper.RadiansToDegrees(_fov);
        set
        {
            float angle = MathHelper.Clamp(value, 1f, 90f);
            // We convert from degrees to radians as soon as the property is set to improve performance.
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }
    
    
    public Camera(Vector3 position, float aspectRatio) : base(position)
    {
        AspectRatio = aspectRatio;
    }


    protected override void OnUpdate(double time)
    {
        if (!IsInputEnabled)
            return;
        
        if (Input.KeyboardState.IsKeyDown(Keys.W))
        {
            Transform.Position += Front * _cameraFlySpeed * (float)time; // Forward
        }

        if (Input.KeyboardState.IsKeyDown(Keys.S))
        {
            Transform.Position -= Front * _cameraFlySpeed * (float)time; // Backwards
        }
        if (Input.KeyboardState.IsKeyDown(Keys.A))
        {
            Transform.Position -= Right * _cameraFlySpeed * (float)time; // Left
        }
        if (Input.KeyboardState.IsKeyDown(Keys.D))
        {
            Transform.Position += Right * _cameraFlySpeed * (float)time; // Right
        }
        if (Input.KeyboardState.IsKeyDown(Keys.Space))
        {
            Transform.Position += Up * _cameraFlySpeed * (float)time; // Up
        }
        if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            Transform.Position -= Up * _cameraFlySpeed * (float)time; // Down
        }

        if (IsMouseFirstMove)
        {
            _mouseLastPos = new Vector2(Input.MouseState.X, Input.MouseState.Y);
            IsMouseFirstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            float deltaX = Input.MouseState.X - _mouseLastPos.X;
            float deltaY = Input.MouseState.Y - _mouseLastPos.Y;
            _mouseLastPos = new Vector2(Input.MouseState.X, Input.MouseState.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            Yaw += deltaX * SENSITIVITY;
            Pitch -= deltaY * SENSITIVITY; // Reversed since y-coordinates range from bottom to top

            if (Input.KeyboardState.IsKeyDown(Keys.LeftControl))
            {
                _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 0.5f;
                _cameraFlySpeed = MathHelper.Clamp(_cameraFlySpeed, 0.1f, 10f);
            }
            else
            {
                // Apply fov
                Fov -= Input.MouseState.ScrollDelta.Y;
            }
        }
    }


    // Get the view matrix using the amazing LookAt function.
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Transform.Position, Transform.Position + _front, Up);
    }

    
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
    }

    
    // This function is going to update the direction vertices using some of the math learned in the web tutorials.
    private void UpdateVectors()
    {
        // First, the front matrix is calculated using some basic trigonometry.
        _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _front.Y = MathF.Sin(_pitch);
        _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        _front = Vector3.Normalize(_front);

        // Calculate both the right and the up vector using cross product.
        // We are calculating the right from the "global" up.
        Right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, _front));
    }
}