namespace BlockEngine.Utils;

public static class Time
{
    public static double DeltaTime { get; private set; }
    public static float DeltaTimeFloat { get; private set; }
    public static double TotalTime { get; private set; }
    
    
    public static void Update(double deltaTime)
    {
        DeltaTime = deltaTime;
        DeltaTimeFloat = (float) deltaTime;
        TotalTime += deltaTime;
    }
    
    
    public static void Reset()
    {
        DeltaTime = 0;
        DeltaTimeFloat = 0;
        TotalTime = 0;
    }
}