using BlockEngine.Utils;

namespace Tests;

[TestFixture]
public class TimeTests
{
    [SetUp]
    public void SetUp()
    {
        Time.Update(0); // Resetting the time before each test
    }

    [Test]
    public void Update_WithPositiveDeltaTime_IncreasesTotalTime()
    {
        double initialTotalTime = Time.TotalTime;
        double deltaTime = 1.0;

        Time.Update(deltaTime);

        Assert.That(Time.TotalTime, Is.EqualTo(initialTotalTime + deltaTime));
    }

    [Test]
    public void Update_WithPositiveDeltaTime_SetsDeltaTime()
    {
        double deltaTime = 1.0;

        Time.Update(deltaTime);

        Assert.That(Time.DeltaTime, Is.EqualTo(deltaTime));
    }

    [Test]
    public void Update_WithZeroDeltaTime_KeepsTotalTimeSame()
    {
        double initialTotalTime = Time.TotalTime;

        Time.Update(0);

        Assert.That(Time.TotalTime, Is.EqualTo(initialTotalTime));
    }

    [Test]
    public void Update_WithZeroDeltaTime_SetsDeltaTimeToZero()
    {
        Time.Update(0);

        Assert.That(Time.DeltaTime, Is.EqualTo(0));
    }

    [Test]
    public void Update_WithNegativeDeltaTime_DecreasesTotalTime()
    {
        double initialTotalTime = Time.TotalTime;
        const double deltaTime = -1.0;

        Time.Update(deltaTime);

        Assert.That(Time.TotalTime, Is.EqualTo(initialTotalTime + deltaTime));
    }

    [Test]
    public void Update_WithNegativeDeltaTime_SetsDeltaTimeToNegative()
    {
        const double deltaTime = -1.0;

        Time.Update(deltaTime);

        Assert.That(Time.DeltaTime, Is.EqualTo(deltaTime));
    }
}