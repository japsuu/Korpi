using OpenTK.Mathematics;

namespace BlockEngine.Utils;

public static class Constants
{
    // Engine
    public const string ENGINE_NAME = "BlockEngine";
    public const string ENGINE_VERSION = "0.0.1";
    
    // IO Paths
    public const string SHADER_PATH = "Assets/Shaders/";
    public const string TEXTURE_PATH = "Assets/Textures/";
    
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
    
    public const int CHUNK_SIZE_WITH_NEIGHBOUR_BORDER = CHUNK_SIZE + 2;
    public const int CHUNK_SIZE_WITH_NEIGHBOUR_BORDER_CUBED = CHUNK_SIZE_WITH_NEIGHBOUR_BORDER * CHUNK_SIZE_WITH_NEIGHBOUR_BORDER * CHUNK_SIZE_WITH_NEIGHBOUR_BORDER;
        
    public const int CHUNK_COLUMN_HEIGHT = 16;
    public const int CHUNK_COLUMN_LOAD_RADIUS = 14;
    public const int CHUNK_COLUMN_LOAD_RADIUS_SQUARED = CHUNK_COLUMN_LOAD_RADIUS * CHUNK_COLUMN_LOAD_RADIUS;
    public const int CHUNK_COLUMN_UNLOAD_RADIUS = 16;
    public const int CHUNK_COLUMN_UNLOAD_RADIUS_SQUARED = CHUNK_COLUMN_UNLOAD_RADIUS * CHUNK_COLUMN_UNLOAD_RADIUS;
}