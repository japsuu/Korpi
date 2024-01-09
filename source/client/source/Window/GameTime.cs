using OpenTK.Mathematics;

namespace Korpi.Client.Window;

public static class GameTime
{
    public static event Action? DayPassed;
    public static event Action? MonthPassed;
    public static event Action? YearPassed;
    
    private static float sunriseStartProgress;
    private static float sunriseEndProgress;
    private static float sunsetStartProgress;
    private static float sunsetEndProgress;
    
    /// <summary>
    /// Time in seconds that has passed since the last frame.
    /// </summary>
    public static double DeltaTime { get; private set; }
    
    /// <summary>
    /// Time in seconds that has passed since the last frame, as a float.
    /// </summary>
    public static float DeltaTimeFloat { get; private set; }
    
    /// <summary>
    /// Time in seconds that has passed since the last fixed frame.
    /// </summary>
    public static double FixedDeltaTime => Constants.FIXED_DELTA_TIME;

    /// <summary>
    /// Time in seconds that has passed since the last fixed frame, as a float.
    /// </summary>
    public static float FixedDeltaTimeFloat => Constants.FIXED_DELTA_TIME;
    
    /// <summary>
    /// Total time in seconds that has passed since the start of the game.
    /// </summary>
    public static double TotalTime { get; private set; }
    
    /// <summary>
    /// Total number of frames that have passed since the start of the game.
    /// </summary>
    public static uint TotalFrameCount { get; private set; }

    /// <summary>
    /// This value stores how far we are in the current update frame, relative to the fixed update loop.
    /// For example, when the value of <see cref="FixedAlpha"/> is 0.5, it means we are halfway between the last frame and the next upcoming frame.
    /// </summary>
    public static double FixedAlpha { get; private set; }
    
    public static int CurrentYear { get; private set; }
    public static int CurrentMonth { get; private set; }
    public static int CurrentDay { get; private set; }
    public static int CurrentHour { get; private set; }
    public static int CurrentMinute { get; private set; }
    public static int CurrentSecond { get; private set; }
    
    /// <summary>
    /// Progress of the current day, from 0.0 to 1.0.
    /// 0.0 is midnight, 0.5 is noon, and 1.0 is midnight again.
    /// </summary>
    public static float DayProgress { get; private set; }
    
    /// <summary>
    /// Progress of the lerp between the day- and night skyboxes, from 0.0 to 1.0.
    /// 0.0 is the night skybox, 1.0 is the day skybox, and anything in between is a lerp.
    /// </summary>
    public static float SkyboxLerpProgress { get; private set; }
    
    public static Vector3 SunDirection { get; private set; }
    
    public static string GetFormattedTime() => $"{CurrentHour:00}:{CurrentMinute:00}:{CurrentSecond:00}";
    public static string GetFormattedDate() => $"{CurrentDay:00}/{CurrentMonth:00}/{CurrentYear:0000}";
    
    
    public static void Initialize()
    {
        CurrentYear = Constants.STARTING_YEAR;
        CurrentMonth = Constants.STARTING_MONTH;
        CurrentDay = Constants.STARTING_DAY;
        CurrentHour = Constants.STARTING_HOUR;
        CurrentMinute = 0;
        CurrentSecond = 0;
        DayProgress = CurrentHour / 24.0f;
        
        sunriseStartProgress = Constants.SUNRISE_START_HOUR / 24.0f;
        sunriseEndProgress = Constants.SUNRISE_END_HOUR / 24.0f;
        sunsetStartProgress = Constants.SUNSET_START_HOUR / 24.0f;
        sunsetEndProgress = Constants.SUNSET_END_HOUR / 24.0f;
    }


    public static void Update(double deltaTime, double fixedAlpha)
    {
        DeltaTime = deltaTime;
        DeltaTimeFloat = (float) deltaTime;
        
        FixedAlpha = fixedAlpha;
        
        TotalTime += deltaTime;
        TotalFrameCount++;
        
        float dayProgression = DeltaTimeFloat / Constants.REAL_SECONDS_PER_GAME_DAY;
        DayProgress += dayProgression;

        // Check if a day has passed.
        if (DayProgress >= 1.0f)
        {
            DayProgress = 0.0f;
            AdvanceDay();
        }

        // Calculate hours, minutes, and seconds.
        double totalSeconds = DayProgress * 24 * 60 * 60;
        CurrentHour = (int)Math.Floor(totalSeconds / 3600);
        CurrentMinute = (int)Math.Floor(totalSeconds % 3600 / 60);
        CurrentSecond = (int)Math.Floor(totalSeconds % 60);
        
        // Update the day-night lerp progress.
        if (DayProgress >= sunriseStartProgress && DayProgress < sunriseEndProgress)
        {
            // We're in the sunrise period.
            float sunriseProgress = (DayProgress - sunriseStartProgress) / (sunriseEndProgress - sunriseStartProgress);
            SkyboxLerpProgress = sunriseProgress;
        }
        else if (DayProgress >= sunsetStartProgress && DayProgress < sunsetEndProgress)
        {
            // We're in the sunset period.
            float sunsetProgress = (DayProgress - sunsetStartProgress) / (sunsetEndProgress - sunsetStartProgress);
            SkyboxLerpProgress = 1.0f - sunsetProgress;
        }
        else if (DayProgress >= sunsetEndProgress || DayProgress < sunriseStartProgress)
        {
            // It's nighttime.
            SkyboxLerpProgress = 0.0f;
        }
        else
        {
            // It's daytime.
            SkyboxLerpProgress = 1.0f;
        }
        
        // Update the sun direction.
        float sunAngle = MathHelper.DegreesToRadians(DayProgress * 360.0f - 90);
        SunDirection = new Vector3((float)Math.Cos(sunAngle), (float)Math.Sin(sunAngle), 0.0f);
    }


    public static void Reset()
    {
        DeltaTime = 0;
        DeltaTimeFloat = 0;
        TotalTime = 0;
        TotalFrameCount = 0;
    }

    
    private static void AdvanceDay()
    {
        CurrentDay++;

        if (CurrentDay > Constants.DAYS_PER_MONTH)
        {
            CurrentDay = 1;
            DayPassed?.Invoke();
            AdvanceMonth();
        }
        else
        {
            DayPassed?.Invoke();
        }
    }

    
    private static void AdvanceMonth()
    {
        CurrentMonth++;

        if (CurrentMonth > Constants.MONTHS_PER_YEAR)
        {
            CurrentMonth = 1;
            MonthPassed?.Invoke();
            AdvanceYear();
        }
        else
        {
            MonthPassed?.Invoke();
        }
    }

    
    private static void AdvanceYear()
    {
        CurrentYear++;
        YearPassed?.Invoke();
    }
}