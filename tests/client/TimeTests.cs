using BlockEngine.Client.Window;

namespace ClientTests;

[TestFixture]
public class TimeTests
{
    [SetUp]
    public void SetUp()
    {
        GameTime.Reset();
    }

    [Test]
    public void Update_WithPositiveDeltaTime_IncreasesTotalTime()
    {
        double initialTotalTime = GameTime.TotalTime;
        double deltaTime = 1.0;
        float fixedAlpha = 0.5f;

        GameTime.Update(deltaTime, fixedAlpha);

        Assert.That(GameTime.TotalTime, Is.EqualTo(initialTotalTime + deltaTime));
    }

    [Test]
    public void Update_WithPositiveDeltaTime_SetsDeltaTime()
    {
        double deltaTime = 1.0;
        float fixedAlpha = 0.5f;

        GameTime.Update(deltaTime, fixedAlpha);

        Assert.That(GameTime.DeltaTime, Is.EqualTo(deltaTime));
    }

    [Test]
    public void Update_WithZeroDeltaTime_KeepsTotalTimeSame()
    {
        double initialTotalTime = GameTime.TotalTime;

        GameTime.Update(0, 0);

        Assert.That(GameTime.TotalTime, Is.EqualTo(initialTotalTime));
    }

    [Test]
    public void Update_WithZeroDeltaTime_SetsDeltaTimeToZero()
    {
        GameTime.Update(0, 0);

        Assert.That(GameTime.DeltaTime, Is.EqualTo(0));
    }

    [Test]
    public void Update_WithNegativeDeltaTime_DecreasesTotalTime()
    {
        double initialTotalTime = GameTime.TotalTime;
        const double deltaTime = -1.0;

        GameTime.Update(deltaTime, 0.5f);

        Assert.That(GameTime.TotalTime, Is.EqualTo(initialTotalTime + deltaTime));
    }
}