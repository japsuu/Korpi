using OpenTK.Mathematics;

namespace BlockEngine.Utils;

public static class Constants
{
    // Engine
    public const string ENGINE_NAME = "BlockEngine";
    public const string ENGINE_VERSION = "0.0.1";
    
    // IO Paths
    public const string SHADER_PATH = "Assets/shaders/";
    public const string TEXTURE_PATH = "Assets/textures/";
    
    // Game logic
    public const uint UPDATE_LOOP_FREQUENCY = 0;
    
    // Coordinates
    public static readonly Vector3 WorldForward = -Vector3.UnitZ;
    public static readonly Vector3 WorldUp = Vector3.UnitY;
    public static readonly Vector3 WorldRight = Vector3.UnitX;
    
    // World generation
    public const int CHUNK_SIZE = 32;
    public const int CHUNK_SIZE_BITMASK = CHUNK_SIZE - 1;
    public const int CHUNK_SIZE_CUBED = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
    public const int CHUNK_SIZE_LOG2 = 5;
    public const int CHUNK_SIZE_LOG2_DOUBLED = 10;
        
    public const int CHUNK_COLUMN_HEIGHT = 16;
    public const int CHUNK_COLUMN_HEIGHT_BLOCKS = CHUNK_COLUMN_HEIGHT * CHUNK_SIZE;
    public const int CHUNK_COLUMN_LOAD_RADIUS = 6;
    public const int CHUNK_COLUMN_LOAD_RADIUS_SQUARED = CHUNK_COLUMN_LOAD_RADIUS * CHUNK_COLUMN_LOAD_RADIUS;
    public const int CHUNK_COLUMN_UNLOAD_RADIUS = 9;
    public const int CHUNK_COLUMN_UNLOAD_RADIUS_SQUARED = CHUNK_COLUMN_UNLOAD_RADIUS * CHUNK_COLUMN_UNLOAD_RADIUS;

    public const int MAX_LIGHT_LEVEL = 31;
    
    public const int MINUTES_PER_DAY = 20;
    public const int SECONDS_PER_DAY = MINUTES_PER_DAY * 60;
    public const int DAYS_IN_MONTH = 30;
    public const float SKYBOX_ROTATION_SPEED_X = 360f / SECONDS_PER_DAY;
    public const float SKYBOX_ROTATION_SPEED_Y = 360f / (SECONDS_PER_DAY * DAYS_IN_MONTH);
}