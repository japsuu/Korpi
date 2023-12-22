namespace BlockEngine.Client.Utils;

public static class Time
{
    public static uint FrameCount { get; private set; }
    public static double DeltaTime { get; private set; }
    public static float DeltaTimeFloat { get; private set; }
    public static double TotalTime { get; private set; }
    
    
    public static void Update(double deltaTime)
    {
        DeltaTime = deltaTime;
        DeltaTimeFloat = (float) deltaTime;
        TotalTime += deltaTime;
        FrameCount++;
    }
    
    
    public static void Reset()
    {
        DeltaTime = 0;
        DeltaTimeFloat = 0;
        TotalTime = 0;
        FrameCount = 0;
    }
}