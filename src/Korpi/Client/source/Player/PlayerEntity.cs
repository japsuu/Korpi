using Korpi.Client.Registries;
using Korpi.Client.Rendering.Cameras;
using KorpiEngine.Core;
using KorpiEngine.Core.ECS.Entities;
using KorpiEngine.Core.InputManagement;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Korpi.Client.Player;

/// <summary>
/// Basic playerEntity entity. Always has a <see cref="PlayerCamera"/> attached to it.
/// </summary>
public class PlayerEntity : TransformEntity
{
    private const float PLAYER_MOVEMENT_SPEED = 8*4f;
    private static readonly Vector3 CameraOffset = new(0, 1.5f, 0);
    private NoclipCamera? _externalCamera;
    private bool _isExternalCameraEnabled;
    
    /// <summary>
    /// The local playerEntity object.
    /// Only one local playerEntity can exist at a time.
    /// </summary>
    public static PlayerEntity LocalPlayerEntity { get; private set; } = null!;
    
    public static ushort SelectedBlockType = 1;
    
    public readonly PlayerCamera Camera;
    
    /// <summary>
    /// The forward vector of the playerEntity's view.
    /// </summary>
    public Vector3 ViewPosition => Camera.Position;
    // public Vector3 ViewPosition => _isExternalCameraEnabled ? _externalCamera!.Position : _camera.Position;
    
    /// <summary>
    /// The forward vector of the playerEntity's view.
    /// </summary>
    public Vector3 ViewForward => Camera.Forward;
    

    /// <summary>
    /// Creates a new player entity at the specified position.
    /// </summary>
    /// <param name="position">Position to spawn the playerEntity at</param>
    /// <param name="cameraYaw">Initial rotation of the playerEntity's camera on the Y axis</param>
    /// <param name="cameraPitch">Initial rotation of the playerEntity's camera on the X axis</param>
    /// <exception cref="Exception">Thrown if the <see cref="LocalPlayerEntity"/> already exists</exception>
    public PlayerEntity(Vector3 position, float cameraYaw, float cameraPitch)
    {
        if (LocalPlayerEntity != null)
            throw new Exception("For now, only one playerEntity can be loaded at a time");
        LocalPlayerEntity = this;
        
        Camera = new PlayerCamera(position + CameraOffset, cameraYaw, cameraPitch);
        
        Transform.LocalPosition = position;
        
        AddComponent(new PlayerRendererComponent(this));
    }


    private void DisableExternalCamera()
    {
        _isExternalCameraEnabled = false;
        _externalCamera!.Dispose();
        _externalCamera = null;
    }


    private void EnableExternalCamera()
    {
        _isExternalCameraEnabled = true;
        _externalCamera = new NoclipCamera(Transform.WorldPosition + CameraOffset, Camera.PitchDegrees, Camera.YawDegrees);
    }


    protected override void OnUpdate()
    {
        if (KorpiEngine.Core.InputManagement.Cursor.IsGrabbed)
        {
            if (Input.KeyboardState.IsKeyPressed(Keys.F5))
            {
                if (_isExternalCameraEnabled)
                    DisableExternalCamera();
                else
                    EnableExternalCamera();
            }
            
            // Update the SelectedBlockType
            if (Input.MouseState.ScrollDelta.Y > 0)
                SelectedBlockType++;
            else if (Input.MouseState.ScrollDelta.Y < 0)
                SelectedBlockType--;
        
            // Clamp between 0 and the number of blocks.
            SelectedBlockType = (ushort)Math.Clamp(SelectedBlockType, 1, BlockRegistry.GetBlockCount() - 1);

            if (_isExternalCameraEnabled)
            {
                _externalCamera!.Update();
                return;
            }

            double time = Time.DeltaTime;
            Vector3 positionDelta = Vector3.Zero;

            if (Input.KeyboardState.IsKeyDown(Keys.W))
            {
                positionDelta += Camera.Forward * PLAYER_MOVEMENT_SPEED * (float)time; // Forward
            }

            if (Input.KeyboardState.IsKeyDown(Keys.S))
            {
                positionDelta += -Camera.Forward * PLAYER_MOVEMENT_SPEED * (float)time; // Backwards
            }

            if (Input.KeyboardState.IsKeyDown(Keys.A))
            {
                positionDelta += -Camera.Right * PLAYER_MOVEMENT_SPEED * (float)time; // Left
            }

            if (Input.KeyboardState.IsKeyDown(Keys.D))
            {
                positionDelta += Camera.Right * PLAYER_MOVEMENT_SPEED * (float)time; // Right
            }

            if (Input.KeyboardState.IsKeyDown(Keys.Space))
            {
                positionDelta += Camera.Up * PLAYER_MOVEMENT_SPEED * (float)time; // Up
            }

            if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                positionDelta += -Camera.Up * PLAYER_MOVEMENT_SPEED * (float)time; // Down
            }

            Transform.WorldPosition += positionDelta;
            Camera.SetPosition(Transform.WorldPosition + CameraOffset);
            Camera.UpdateRotation();
        }
        Transform.WorldRotation = new Vector3(0, -Camera.YawRadians, 0);
    }
}