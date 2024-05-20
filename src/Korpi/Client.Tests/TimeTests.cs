using Korpi.Client;
using KorpiEngine.Core;

namespace Client.Tests;

[TestFixture]
public class TimeTests
{
    [SetUp]
    public void SetUp()
    {
        Time.Reset();
    }

    [Test]
    public void Update_WithPositiveDeltaTime_IncreasesTotalTime()
    {
        double initialTotalTime = Time.TotalTime;
        double deltaTime = 1.0;
        float fixedAlpha = 0.5f;

        Time.Update(deltaTime, fixedAlpha);

        Assert.That(Time.TotalTime, Is.EqualTo(initialTotalTime + deltaTime));
    }

    [Test]
    public void Update_WithPositiveDeltaTime_SetsDeltaTime()
    {
        double deltaTime = 1.0;
        float fixedAlpha = 0.5f;

        Time.Update(deltaTime, fixedAlpha);

        Assert.That(Time.DeltaTime, Is.EqualTo(deltaTime));
    }

    [Test]
    public void Update_WithZeroDeltaTime_KeepsTotalTimeSame()
    {
        double initialTotalTime = Time.TotalTime;

        Time.Update(0, 0);

        Assert.That(Time.TotalTime, Is.EqualTo(initialTotalTime));
    }

    [Test]
    public void Update_WithZeroDeltaTime_SetsDeltaTimeToZero()
    {
        Time.Update(0, 0);

        Assert.That(Time.DeltaTime, Is.EqualTo(0));
    }

    [Test]
    public void Update_WithNegativeDeltaTime_DecreasesTotalTime()
    {
        double initialTotalTime = Time.TotalTime;
        const double deltaTime = -1.0;

        Time.Update(deltaTime, 0.5f);

        Assert.That(Time.TotalTime, Is.EqualTo(initialTotalTime + deltaTime));
    }
}