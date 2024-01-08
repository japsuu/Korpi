using BlockEngine.Client.Mathematics;

namespace BlockEngine.Client.Window;

/// <summary>
/// Class that can dynamically adjust game settings based on the current performance of the game.
/// </summary>
public static class DynamicPerformance
{
    /// <summary>
    /// The FPS we are targeting.
    /// Above this FPS the game will be considered running at 100% performance.
    /// Below this FPS the game performance is considered degraded, lowering the <see cref="CurrentPerformance"/> score.
    /// The actual FPS may be higher than this.
    /// </summary>
    private const int FPS_TARGET = 100;
    
    /// <summary>
    /// Current performance of the game.
    /// 0 = lowest performance, 1 = target performance, 1+ = higher performance.
    /// </summary>
    public static float CurrentPerformance { get; private set; } = 1f;
    
    /// <summary>
    /// Current performance of the game, clamped to 0-1 range.
    /// 0 = lowest performance, 1 = target performance.
    /// </summary>
    public static float CurrentPerformanceClamped { get; private set; } = 1f;
    
    
    /// <summary>
    /// Calculates the current performance of the game based on the given delta time.
    /// </summary>
    /// <param name="deltaTime">Delta time in seconds.</param>
    public static void Update(double deltaTime)
    {
        float fps = 1f / (float)deltaTime;
        CurrentPerformance = fps / FPS_TARGET;
        CurrentPerformanceClamped = Math.Clamp(CurrentPerformance, 0f, 1f);
    }
    
    
    /// <summary>
    /// Gets a dynamic float based on the current performance.
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="allowOvershoot">Whether to allow the value to overshoot <see cref="max"/> (if <see cref="CurrentPerformance"/> is higher than 1).</param>
    /// <returns></returns>
    public static float GetDynamic(float min, float max, bool allowOvershoot = false)
    {
        float factor = allowOvershoot ? CurrentPerformance : CurrentPerformanceClamped;
        return MathUtils.LerpUnclamped(min, max, factor);
    }
    
    
    /// <summary>
    /// Gets a dynamic int based on the current performance.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive).</param>
    /// <param name="allowOvershoot">Whether to allow the value to overshoot <see cref="max"/> (if <see cref="CurrentPerformance"/> is higher than 1).</param>
    /// <returns></returns>
    public static int GetDynamic(int min, int max, bool allowOvershoot = false)
    {
        float factor = allowOvershoot ? CurrentPerformance : CurrentPerformanceClamped;
        return (int)Math.Round(MathUtils.LerpUnclamped(min, max, factor));
    }
}