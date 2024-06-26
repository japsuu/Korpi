﻿using KorpiEngine.Core.Threading.Jobs;

namespace Korpi.Client.Configuration;

/// <summary>
/// Contains constants used throughout the client.
/// </summary>
public static class Constants
{
    #region CLIENT

    /// <summary>
    /// Name of the client, displayed on the window title.
    /// </summary>
    public const string CLIENT_NAME = "Korpi Client";

    /// <summary>
    /// The current of the client.
    /// TODO: Change to a Version struct.
    /// </summary>
    public const string CLIENT_VERSION = "0.0.1";

    #endregion

    #region IO PATHS

    /// <summary>
    /// Relative path to the core shaders directory.
    /// </summary>
    public const string SHADER_BASE_PATH = "assets/shaders";

    /// <summary>
    /// Relative path to the textures directory.
    /// </summary>
    public const string TEXTURE_PATH = "assets/textures/";

    /// <summary>
    /// Relative path to the mods directory.
    /// </summary>
    public const string MODS_PATH = "assets/mods/";

    #endregion

    #region MODDING

    /// <summary>
    /// The namespace used for built-in mods.
    /// </summary>
    public const string BUILT_IN_MOD_NAMESPACE = "korpi";

    /// <summary>
    /// The file extension used for yaml mod files.
    /// </summary>
    public const string YAML_MOD_FILE_EXTENSION = "yaml";

    #endregion

    #region GAME LOOP

    /// <summary>
    /// The maximum number of frames to be rendered per second.
    /// 0 = Unlimited.
    /// </summary>
    public const int UPDATE_FRAME_FREQUENCY = 0;

    #endregion

    #region CHUNK SIZE

    /// <summary>
    /// The length of one side of a chunk in blocks.
    /// </summary>
    public const int CHUNK_SIDE_LENGTH = 32;

    /// <summary>
    /// The amount of bits needed to represent <see cref="CHUNK_SIDE_LENGTH"/>.
    /// </summary>
    public const int CHUNK_SIDE_BITS = 5;

    /// <summary>
    /// The amount of chunks in a chunk column.
    /// </summary>
    public const int CHUNK_COLUMN_HEIGHT_CHUNKS = 16;
    
    /// <summary>
    /// The height of a chunk in blocks.
    /// </summary>
    public const int CHUNK_COLUMN_HEIGHT_BLOCKS = CHUNK_COLUMN_HEIGHT_CHUNKS * CHUNK_SIDE_LENGTH;
    
    /// <summary>
    /// The count of block in a chunk.
    /// </summary>
    public const int CHUNK_BLOCKS_COUNT = CHUNK_SIDE_LENGTH * CHUNK_SIDE_LENGTH * CHUNK_COLUMN_HEIGHT_BLOCKS;

    #endregion

    #region CHUNK LOADING

    /// <summary>
    /// Whether or not to load chunks in a circular or rectangular region around the player.
    /// </summary>
    public const bool CIRCULAR_LOAD_REGION = true;
    
    /// <summary>
    /// The radius (in chunks) around the player to load chunks.
    /// </summary>
    public const int CHUNK_LOAD_RADIUS = 14;
    
    /// <summary>
    /// The radius (in chunks) around the player to unload chunks.
    /// Should be greater than <see cref="CHUNK_LOAD_RADIUS"/>.
    /// </summary>
    public const int CHUNK_UNLOAD_RADIUS = CHUNK_LOAD_RADIUS + 2;

    #endregion

    #region TIME

    /// <summary>
    /// The amount of real seconds that pass per in-game day.
    /// </summary>
    public const int REAL_SECONDS_PER_GAME_DAY = 60*20;
    
    /// <summary>
    /// The amount of in-game days that pass per in-game month.
    /// </summary>
    public const int DAYS_PER_MONTH = 30;
    
    /// <summary>
    /// The amount of in-game months that pass per in-game year.
    /// </summary>
    public const int MONTHS_PER_YEAR = 12;
    
    /// <summary>
    /// The year the game starts in.
    /// </summary>
    public const int STARTING_YEAR = 783;
    
    /// <summary>
    /// The month the game starts in.
    /// </summary>
    public const int STARTING_MONTH = 5;
    
    /// <summary>
    /// The day the game starts in.
    /// </summary>
    public const int STARTING_DAY = 1;
    
    /// <summary>
    /// The hour the game starts in.
    /// </summary>
    public const int STARTING_HOUR = 12;
    
    /// <summary>
    /// The hour the sunrise event starts.
    /// </summary>
    public const int SUNRISE_START_HOUR = 4;
    
    /// <summary>
    /// The hour the sunrise event ends.
    /// </summary>
    public const int SUNRISE_END_HOUR = 7;
    
    /// <summary>
    /// The hour the sunset event starts.
    /// </summary>
    public const int SUNSET_START_HOUR = 17;
    
    /// <summary>
    /// The hour the sunset event ends.
    /// </summary>
    public const int SUNSET_END_HOUR = 20;

    #endregion

    #region SKYBOX

    /// <summary>
    /// How many seconds it takes for the skybox to rotate 360 degrees on the X axis.
    /// </summary>
    public const float SKYBOX_ROTATION_SPEED_X = 360f / REAL_SECONDS_PER_GAME_DAY;
    
    /// <summary>
    /// How many seconds it takes for the skybox to rotate 360 degrees on the Y axis.
    /// </summary>
    public const float SKYBOX_ROTATION_SPEED_Y = 360f / (REAL_SECONDS_PER_GAME_DAY * DAYS_PER_MONTH);

    #endregion

    #region RENDERING

    /// <summary>
    /// The maximum light level a block can have.
    /// </summary>
    public const int MAX_LIGHT_LEVEL = 31;
    
    /// <summary>
    /// The size of a block texture in pixels.
    /// </summary>
    public const int BLOCK_TEXTURE_SIZE = 32;

    #endregion

    #region THREADING

    /// <summary>
    /// Maximum amount of time (ms) a <see cref="KorpiJob{T}"/> will wait for a lock before aborting.
    /// </summary>
    public const int JOB_LOCK_TIMEOUT_MS = 10000;

    #endregion
}