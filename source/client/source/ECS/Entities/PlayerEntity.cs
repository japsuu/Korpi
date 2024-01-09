using Korpi.Client.ECS.Components;
using Korpi.Client.Registries;
using Korpi.Client.Rendering.Cameras;
using Korpi.Client.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Korpi.Client.ECS.Entities;

/// <summary>
/// Basic playerEntity entity. Always has a <see cref="PlayerCamera"/> attached to it.
/// </summary>
public class PlayerEntity : TransformEntity
{
    private const float PLAYER_MOVEMENT_SPEED = 8*4f;
    private static readonly Vector3 CameraOffset = new(0, 1.5f, 0);
    
    private readonly PlayerCamera _camera;
    private NoclipCamera? _externalCamera;
    private bool _isExternalCameraEnabled;
    
    /// <summary>
    /// The local playerEntity object.
    /// Only one local playerEntity can exist at a time.
    /// </summary>
    public static PlayerEntity LocalPlayerEntity { get; private set; } = null!;
    
    public static ushort SelectedBlockType = 1;
    
    /// <summary>
    /// The forward vector of the playerEntity's view.
    /// </summary>
    public Vector3 ViewPosition => _camera.Position;
    // public Vector3 ViewPosition => _isExternalCameraEnabled ? _externalCamera!.Position : _camera.Position;
    
    /// <summary>
    /// The forward vector of the playerEntity's view.
    /// </summary>
    public Vector3 ViewForward => _camera.Forward;
    

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
        
        _camera = new PlayerCamera(position + CameraOffset, cameraYaw, cameraPitch);
        
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
        _externalCamera = new NoclipCamera(Transform.WorldPosition + CameraOffset, _camera.PitchDegrees, _camera.YawDegrees);
    }


    protected override void OnUpdate()
    {
        if (!GameClient.IsPlayerInGui)
        {
            if (Input.KeyboardState.IsKeyPressed(Keys.F5))
            {
                if (_isExternalCameraEnabled)
                    DisableExternalCamera();
                else
                    EnableExternalCamera();
            }

            if (_isExternalCameraEnabled)
            {
                _externalCamera!.Update();
                return;
            }

            double time = GameTime.DeltaTime;
            Vector3 positionDelta = Vector3.Zero;

            if (Input.KeyboardState.IsKeyDown(Keys.W))
            {
                positionDelta += _camera.Forward * PLAYER_MOVEMENT_SPEED * (float)time; // Forward
            }

            if (Input.KeyboardState.IsKeyDown(Keys.S))
            {
                positionDelta += -_camera.Forward * PLAYER_MOVEMENT_SPEED * (float)time; // Backwards
            }

            if (Input.KeyboardState.IsKeyDown(Keys.A))
            {
                positionDelta += -_camera.Right * PLAYER_MOVEMENT_SPEED * (float)time; // Left
            }

            if (Input.KeyboardState.IsKeyDown(Keys.D))
            {
                positionDelta += _camera.Right * PLAYER_MOVEMENT_SPEED * (float)time; // Right
            }

            if (Input.KeyboardState.IsKeyDown(Keys.Space))
            {
                positionDelta += _camera.Up * PLAYER_MOVEMENT_SPEED * (float)time; // Up
            }

            if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                positionDelta += -_camera.Up * PLAYER_MOVEMENT_SPEED * (float)time; // Down
            }

            Transform.WorldPosition += positionDelta;
            _camera.SetPosition(Transform.WorldPosition + CameraOffset);
            _camera.UpdateRotation();
            
            // Update the SelectedBlockType
            if (Input.MouseState.ScrollDelta.Y > 0)
                SelectedBlockType++;
            else if (Input.MouseState.ScrollDelta.Y < 0)
                SelectedBlockType--;
        
            // Clamp between 0 and the number of blocks.
            SelectedBlockType = (ushort)System.Math.Clamp(SelectedBlockType, 1, BlockRegistry.GetBlockCount() - 1);
        }
        Transform.WorldRotation = new Vector3(0, -_camera.YawRadians, 0);
    }
}