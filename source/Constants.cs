using OpenTK.Mathematics;

namespace BlockEngine;

public static class Constants
{
    // Engine
    public const string ENGINE_NAME = "BlockEngine";
    public const string ENGINE_VERSION = "0.0.1";
    
    // IO Paths
    public const string SHADER_PATH = "Assets/shaders/";
    public const string TEXTURE_PATH = "Assets/textures/";
    public const string MODS_PATH = "Assets/mods/";
    
    // Mods
    public const string BUILT_INS_NAMESPACE = "block_engine";
    public const string YAML_MOD_FILE_EXTENSION = "yaml";
    
    // Game logic
    public const uint UPDATE_LOOP_FREQUENCY = 0;
    
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

    // Time
    public const int REAL_SECONDS_PER_GAME_DAY = 1200;
    public const int DAYS_PER_MONTH = 30;
    public const int MONTHS_PER_YEAR = 12;
    public const int STARTING_YEAR = 1;
    public const int STARTING_MONTH = 1;
    public const int STARTING_DAY = 1;
    public const int STARTING_HOUR = 12;
    public const int SUNRISE_START_HOUR = 6;
    public const int SUNRISE_END_HOUR = 8;
    public const int SUNSET_START_HOUR = 18;
    public const int SUNSET_END_HOUR = 20;
    
    
    // Skybox
    public const float SKYBOX_ROTATION_SPEED_X = 360f / REAL_SECONDS_PER_GAME_DAY;
    public const float SKYBOX_ROTATION_SPEED_Y = 360f / (REAL_SECONDS_PER_GAME_DAY * DAYS_PER_MONTH);
    
    // Rendering
    public const int MAX_LIGHT_LEVEL = 31;
    public const int MAX_SUPPORTED_TEXTURES = 1024;
    public const int BLOCK_TEXTURE_SIZE = 32;
    public const float ANISOTROPIC_FILTERING_LEVEL = 16f;
}