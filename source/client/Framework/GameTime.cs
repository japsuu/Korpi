using BlockEngine.Client.Utils;

namespace BlockEngine.Client.Framework;

public static class GameTime
{
    public static event Action? DayPassed;
    public static event Action? MonthPassed;
    public static event Action? YearPassed;
    
    public static event Action? SunriseStarting;
    public static event Action? SunsetStarting;
    
    public static int CurrentYear { get; private set; }
    public static int CurrentMonth { get; private set; }
    public static int CurrentDay { get; private set; }
    public static int CurrentHour { get; private set; }
    public static int CurrentMinute { get; private set; }
    public static int CurrentSecond { get; private set; }
    
    public static float DayProgress { get; private set; }
    
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
    }


    public static void Update()
    {
        float dayProgression = Time.DeltaTimeFloat / Constants.REAL_SECONDS_PER_GAME_DAY;
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

        // Check if sunrise or sunset is starting.
        switch (CurrentHour)
        {
            case Constants.SUNRISE_START_HOUR:
                SunriseStarting?.Invoke();
                break;
            case Constants.SUNSET_START_HOUR:
                SunsetStarting?.Invoke();
                break;
        }
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