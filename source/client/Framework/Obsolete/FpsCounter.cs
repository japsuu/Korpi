namespace BlockEngine.Client.Framework.Obsolete;

[Obsolete("Use the ImGui perf window instead")]
public class FpsCounter
{
    private const double UPDATE_INTERVAL = 0.2;
    
    public double Fps { get; private set; }
    
    private double _accumulator;
    
    
    public void Update(double deltaTime)
    {
        _accumulator += deltaTime;

        if (_accumulator < UPDATE_INTERVAL)
            return;
        
        Fps = 1.0 / deltaTime;

        _accumulator = 0;
    }
}