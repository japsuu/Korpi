using System.Diagnostics;

namespace Korpi.Client.Debugging;

/// <summary>
/// A scope that can be used to profile code.
/// Wrap the code you want to profile in a using statement.
/// </summary>
public sealed class ProfileScope : IDisposable
{
    public ProfileScope(string name)
    {
        KorpiProfiler.Start(name);
    }

    public void Dispose()
    {
        KorpiProfiler.End();
    }
}

/// <summary>
/// A simple profiler that can be used to measure the duration of code execution.
/// NOTE: This is not thread-safe.
/// </summary>
public static class KorpiProfiler
{
    private static Profile? lastFrame;
    private static readonly Stack<Profile> Profiles = new();
    
    
    public static Profile? GetLastFrame() => lastFrame;


    public static void StartFrame()
    {
        Start("Frame");
    }


    public static void Start(string name)
    {
        if (Profiles.Count == 0 && name != "Frame")
            throw new InvalidOperationException("Cannot call Start before StartFrame.");
        
        Profiles.Push(new Profile(name, Stopwatch.StartNew()));
    }


    public static void End()
    {
        if (Profiles.Count == 1)
            throw new InvalidOperationException("Cannot call End without a matching Start.");

        Profile profile = Profiles.Pop();
        profile.Stopwatch.Stop();
        profile.Duration = profile.Stopwatch.Elapsed;

        if (Profiles.Count > 0)
            Profiles.Peek().Children.Add(profile);
    }
    
    
    public static void EndFrame()
    {
        if (Profiles.Count > 1)
            throw new InvalidOperationException("Cannot end frame while there are active profiles.");

        Profile profile = Profiles.Pop();
        profile.Stopwatch.Stop();
        profile.Duration = profile.Stopwatch.Elapsed;
        lastFrame = profile;
    }
}

public class Profile
{
    public readonly string Name;
    public readonly Stopwatch Stopwatch;
    public readonly List<Profile> Children = new();
    public TimeSpan Duration { get; set; }


    public Profile(string name, Stopwatch stopwatch)
    {
        Name = name;
        Stopwatch = stopwatch;
    }
}