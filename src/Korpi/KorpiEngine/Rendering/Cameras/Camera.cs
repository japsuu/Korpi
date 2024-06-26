﻿using KorpiEngine.Core.Platform;
using OpenTK.Mathematics;

namespace KorpiEngine.Rendering.Cameras;

/// <summary>
/// The base class for a camera used to render the scene.
/// </summary>
public abstract class Camera : IDisposable, IComparable<Camera>
{
    /// <summary>
    /// The current camera rendering to the screen.
    /// </summary>
    public static Camera RenderingCamera { get; private set; } = null!;
    
    /// <summary>
    /// All cameras currently active.
    /// </summary>
    private static SortedSet<Camera> ActiveCameras { get; } = new();
    
    /// <summary>
    /// The render priority of this camera.
    /// Only the camera with the lowest render priority will be rendered.
    /// </summary>
    protected int RenderPriority;
    
    public const float DEPTH_NEAR = 0.01f;
    public const float DEPTH_FAR = 1000f;

    public readonly Frustum ViewFrustum;
    
    /// <summary>
    /// The view matrix of this camera.
    /// </summary>
    public Matrix4 ViewMatrix { get; private set; }
    
    /// <summary>
    /// The projection matrix of this camera.
    /// </summary>
    public Matrix4 ProjectionMatrix { get; private set; }
    
    /// <summary>
    /// Local position of the camera.
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <summary>
    /// Forward vector pointing outwards from the camera.
    /// </summary>
    public Vector3 Forward { get; private set; }

    /// <summary>
    /// Up vector pointing upwards from the camera.
    /// </summary>
    public Vector3 Up { get; private set; }

    /// <summary>
    /// Right vector pointing to the right of the camera.
    /// </summary>
    public Vector3 Right { get; private set; }

    /// <summary>
    /// Rotation around the X axis (radians)
    /// </summary>
    public float PitchRadians { get; private set; }

    /// <summary>
    /// Rotation around the X axis (degrees)
    /// </summary>
    public float PitchDegrees
    {
        get => MathHelper.RadiansToDegrees(PitchRadians);
        set
        {
            // We clamp the pitch value between -89 and 89 to prevent gimbal lock and/or the camera from going upside down.
            float angle = MathHelper.Clamp(value, -89f, 89f);
            // We convert from degrees to radians as soon as the property is set to improve performance.
            PitchRadians = MathHelper.DegreesToRadians(angle);
            UpdateDirectionVectors();
        }
    }

    /// <summary>
    /// Rotation around the Y axis (radians)
    /// </summary>
    public float YawRadians { get; private set; }

    /// <summary>
    /// Rotation around the Y axis (degrees)
    /// </summary>
    public float YawDegrees
    {
        get => MathHelper.RadiansToDegrees(YawRadians);
        set
        {
            // We convert from degrees to radians as soon as the property is set to improve performance.
            YawRadians = MathHelper.DegreesToRadians(value);
            UpdateDirectionVectors();
        }
    }

    /// <summary>
    /// The field of view of the camera (radians)
    /// </summary>
    public float FovRadians { get; private set; }

    /// <summary>
    /// The field of view (FOV degrees, the vertical angle of the camera view).
    /// </summary>
    public float FovDegrees
    {
        get => MathHelper.RadiansToDegrees(FovRadians);
        set
        {
            float angle = MathHelper.Clamp(value, 1f, 90f);
            // We convert from degrees to radians as soon as the property is set to improve performance.
            FovRadians = MathHelper.DegreesToRadians(angle);
            RecalculateProjectionMatrix();
        }
    }


    protected Camera(Vector3 localPosition, int renderPriority = 0)
    {
        ViewFrustum = new Frustum();
        RenderPriority = renderPriority;
        FovRadians = MathHelper.PiOver2;
        Position = localPosition;
        YawRadians = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.
        Forward = -Vector3.UnitZ;
        Up = Vector3.UnitY;
        Right = Vector3.UnitX;

        WindowInfo.ClientResized += OnClientResized;
        
        Initialize();
        ActiveCameras.Add(this);
        RenderingCamera = ActiveCameras.Min!;
    }


    protected Camera(Vector3 localPosition, float pitch, float yaw, int renderPriority = 0)
    {
        ViewFrustum = new Frustum();
        RenderPriority = renderPriority;
        FovRadians = MathHelper.PiOver2;
        Position = localPosition;
        YawRadians = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.
        Forward = -Vector3.UnitZ;
        Up = Vector3.UnitY;
        Right = Vector3.UnitX;
        PitchDegrees = pitch;
        YawDegrees = yaw;

        WindowInfo.ClientResized += OnClientResized;
        
        Initialize();
        ActiveCameras.Add(this);
        RenderingCamera = ActiveCameras.Min!;
    }
    
    
    /// <summary>
    /// Sets the position of the camera and recalculates the view matrix.
    /// </summary>
    /// <param name="position">The new position</param>
    public void SetPosition(Vector3 position)
    {
        Position = position;
        RecalculateViewMatrix();
    }
    
    
    protected abstract Vector3 CalculateForwardVector(float pitch, float yaw);
    protected abstract Vector3 CalculateRightVector(Vector3 forward);
    protected abstract Vector3 CalculateUpVector(Vector3 right, Vector3 forward);

    
    /// <summary>
    /// Updates the direction vectors (<see cref="Forward"/>, <see cref="Right"/>, <see cref="Up"/>) of the camera.
    /// </summary>
    private void UpdateDirectionVectors()
    {
        Vector3 forward = CalculateForwardVector(PitchRadians, YawRadians);
        Vector3 right = CalculateRightVector(forward);
        Vector3 up = CalculateUpVector(right, forward);
        
        Forward = forward;
        Right = right;
        Up = up;
        RecalculateViewMatrix();
    }


    private void OnClientResized(WindowInfo.WindowResizeEventArgs windowResizeEventArgs)
    {
        RecalculateProjectionMatrix();
    }


    private void Initialize()
    {
        RecalculateViewMatrix();
        RecalculateProjectionMatrix();
        RecalculateFrustum();
    }


    /// <summary>
    /// Calculates the view matrix of the camera using a LookAt function.
    /// </summary>
    private void RecalculateViewMatrix()
    {
        ViewMatrix = Matrix4.LookAt(Position, Position + Forward, Up);
        RecalculateFrustum();
    }

    
    /// <summary>
    /// Calculates the projection matrix of the camera using a perspective projection.
    /// </summary>
    private void RecalculateProjectionMatrix()
    {
        ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FovRadians, WindowInfo.ClientAspectRatio, DEPTH_NEAR, DEPTH_FAR);
        RecalculateFrustum();
    }


    private void RecalculateFrustum()
    {
        Matrix4 viewProjection = ViewMatrix * ProjectionMatrix;

        // Left plane.
        ViewFrustum.Left.Normal = new Vector3(viewProjection.M14 + viewProjection.M11, viewProjection.M24 + viewProjection.M21, viewProjection.M34 + viewProjection.M31);
        ViewFrustum.Left.Distance = viewProjection.M44 + viewProjection.M41;

        // Right plane.
        ViewFrustum.Right.Normal = new Vector3(viewProjection.M14 - viewProjection.M11, viewProjection.M24 - viewProjection.M21, viewProjection.M34 - viewProjection.M31);
        ViewFrustum.Right.Distance = viewProjection.M44 - viewProjection.M41;

        // Bottom plane.
        ViewFrustum.Bottom.Normal = new Vector3(viewProjection.M14 + viewProjection.M12, viewProjection.M24 + viewProjection.M22, viewProjection.M34 + viewProjection.M32);
        ViewFrustum.Bottom.Distance = viewProjection.M44 + viewProjection.M42;

        // Top plane.
        ViewFrustum.Top.Normal = new Vector3(viewProjection.M14 - viewProjection.M12, viewProjection.M24 - viewProjection.M22, viewProjection.M34 - viewProjection.M32);
        ViewFrustum.Top.Distance = viewProjection.M44 - viewProjection.M42;

        // Near plane.
        ViewFrustum.Near.Normal = new Vector3(viewProjection.M13, viewProjection.M23, viewProjection.M33);
        ViewFrustum.Near.Distance = viewProjection.M43;

        // Far plane.
        ViewFrustum.Far.Normal = new Vector3(viewProjection.M14 - viewProjection.M13, viewProjection.M24 - viewProjection.M23, viewProjection.M34 - viewProjection.M33);
        ViewFrustum.Far.Distance = viewProjection.M44 - viewProjection.M43;

        // Normalize the planes.
        for (int i = 0; i < 6; i++)
        {
            float length = ViewFrustum.Planes[i].Normal.Length;
            ViewFrustum.Planes[i].Normal /= length;
            ViewFrustum.Planes[i].Distance /= length;
        }
    }
    
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        // Dispose managed resources.
        ActiveCameras.Remove(this);
        RenderingCamera = ActiveCameras.Min!;
        WindowInfo.ClientResized -= OnClientResized;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    public int CompareTo(Camera? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (ReferenceEquals(null, other))
            return 1;
        return RenderPriority.CompareTo(other.RenderPriority);
    }
    
    
    ~Camera()
    {
        Dispose(false);
    }
}