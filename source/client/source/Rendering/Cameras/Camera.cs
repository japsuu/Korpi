﻿using Korpi.Client.Window;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Cameras;

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
        RenderPriority = renderPriority;
        FovRadians = MathHelper.PiOver2;
        Position = localPosition;
        YawRadians = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.
        Forward = -Vector3.UnitZ;
        Up = Vector3.UnitY;
        Right = Vector3.UnitX;

        GameClient.ClientResized += RecalculateProjectionMatrix;
        
        RecalculateMatrices();
        ActiveCameras.Add(this);
        RenderingCamera = ActiveCameras.Min!;
    }


    protected Camera(Vector3 localPosition, float pitch, float yaw, int renderPriority = 0)
    {
        RenderPriority = renderPriority;
        FovRadians = MathHelper.PiOver2;
        Position = localPosition;
        YawRadians = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.
        Forward = -Vector3.UnitZ;
        Up = Vector3.UnitY;
        Right = Vector3.UnitX;
        PitchDegrees = pitch;
        YawDegrees = yaw;

        GameClient.ClientResized += RecalculateProjectionMatrix;
        
        RecalculateMatrices();
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


    private void RecalculateMatrices()
    {
        RecalculateViewMatrix();
        RecalculateProjectionMatrix();
    }


    /// <summary>
    /// Calculates the view matrix of the camera using a LookAt function.
    /// </summary>
    private void RecalculateViewMatrix()
    {
        ViewMatrix = Matrix4.LookAt(Position, Position + Forward, Up);
    }

    
    /// <summary>
    /// Calculates the projection matrix of the camera using a perspective projection.
    /// </summary>
    private void RecalculateProjectionMatrix()
    {
        ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FovRadians, GameClient.WindowAspectRatio, 0.01f, 1000f);
    }
    
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        // Dispose managed resources.
        ActiveCameras.Remove(this);
        RenderingCamera = ActiveCameras.Min!;
        GameClient.ClientResized -= RecalculateProjectionMatrix;
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