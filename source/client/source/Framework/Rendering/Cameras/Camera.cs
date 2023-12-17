using BlockEngine.Client.Framework.ECS.Entities;
using BlockEngine.Client.Framework.Rendering.ImGuiWindows;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Client.Framework.Rendering.Cameras;

public class Camera : TransformEntity
{
    public static Camera ActiveCamera { get; private set; } = null!;

    private const float SENSITIVITY = 0.2f;
    
    private bool _isActive;
    
    private CameraWindow _cameraWindow;
    
    private float _cameraFlySpeed = 1.5f;
    
    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;
    
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

    public Vector3 Up
    {
        get => _up;
        private set => _up = value;
    }

    public Vector3 Right
    {
        get => _right;
        private set => _right = value;
    }

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
    
    
    public Camera(Vector3 localPosition, float aspectRatio) : base(localPosition)
    {
        AspectRatio = aspectRatio;
        _cameraWindow = new CameraWindow(this);

        SetActive(true);
    }
    
    
    public Camera(Vector3 localPosition, float pitch, float yaw, float aspectRatio) : base(localPosition)
    {
        AspectRatio = aspectRatio;
        _cameraWindow = new CameraWindow(this);
        Pitch = pitch;
        Yaw = yaw;

        SetActive(true);
    }
    
    
    public void SetActive(bool cancelIfActiveExists = false)
    {
        if (_isActive)
            return;
        
        if (ActiveCamera != null)
        {
            if (cancelIfActiveExists)
                return;
            
            ActiveCamera._isActive = false;
        }

        ActiveCamera = this;
        _isActive = true;
    }


    // TODO: Project the Front/Right vectors onto the XZ plane while keeping their magnitude and use those for movement.
    // TODO: Use world up/down instead of camera up/down for vertical movement.
    // TODO: Possibly use the Transform.forward property.
    protected override void OnUpdate(double time)
    {
        if (!IsInputEnabled)
            return;
        
        if (Input.KeyboardState.IsKeyDown(Keys.W))
        {
            Transform.LocalPosition += Front * _cameraFlySpeed * (float)time; // Forward
        }

        if (Input.KeyboardState.IsKeyDown(Keys.S))
        {
            Transform.LocalPosition -= Front * _cameraFlySpeed * (float)time; // Backwards
        }

        if (Input.KeyboardState.IsKeyDown(Keys.A))
        {
            Transform.LocalPosition -= Right * _cameraFlySpeed * (float)time; // Left
        }

        if (Input.KeyboardState.IsKeyDown(Keys.D))
        {
            Transform.LocalPosition += Right * _cameraFlySpeed * (float)time; // Right
        }

        if (Input.KeyboardState.IsKeyDown(Keys.Space))
        {
            Transform.LocalPosition += Up * _cameraFlySpeed * (float)time; // Up
        }

        if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            Transform.LocalPosition -= Up * _cameraFlySpeed * (float)time; // Down
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
                // Apply fov
                Fov -= Input.MouseState.ScrollDelta.Y;
            }
            else
            {
                switch (_cameraFlySpeed)
                {
                    // Changing the fly speed should be accurate at the lower end, but fast when at the upper end.
                    case <= 1f:
                        _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 0.05f;
                        break;
                    case <= 5f:
                    {
                        _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 0.5f;

                        if (_cameraFlySpeed < 1f)
                        {
                            _cameraFlySpeed = 0.95f;
                        }

                        break;
                    }
                    case <= 10f:
                    {
                        _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 1f;

                        if (_cameraFlySpeed < 5f)
                        {
                            _cameraFlySpeed = 4.5f;
                        }

                        break;
                    }
                    default:
                    {
                        _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 5f;

                        if (_cameraFlySpeed < 10f)
                        {
                            _cameraFlySpeed = 9f;
                        }

                        break;
                    }
                }

                _cameraFlySpeed = MathHelper.Clamp(_cameraFlySpeed, 0.05f, 50f);
            }
        }
    }


    // Get the view matrix using the amazing LookAt function.
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Transform.LocalPosition, Transform.LocalPosition + _front, _up);
    }

    
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 1000f);
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
        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }


    public string GetFlySpeedFormatted()
    {
        if (_cameraFlySpeed <= 1f)
        {
            return $"{_cameraFlySpeed:F2}";
        }

        if (_cameraFlySpeed <= 5f)
        {
            return $"{_cameraFlySpeed:F1}";
        }
        
        return $"{_cameraFlySpeed:F0}";
    }
}