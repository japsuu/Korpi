using BlockEngine.Framework.Registries;

namespace Tests;

[TestFixture]
public class NamespacedIdRegistryTests
{
    /*private class Mock : IHasId
    {
        public int Id { get; private set; } = -1;
        
        public void AssignId(ushort id) => Id = id;
    }
    
    private BlockRegistry<IHasId> _registry = null!;

    [SetUp]
    public void SetUp()
    {
        _registry = new BlockRegistry<IHasId>("TestRegistry");
    }

    [Test]
    public void Register_AddsValueToRegistry_ReturnsSameValue()
    {
        Mock mockValue = new();
        IHasId result = _registry.Register("test", mockValue);

        Assert.That(result, Is.EqualTo(mockValue));
    }

    [Test]
    public void Register_AddsValueToRegistry_AssignsIdToValue()
    {
        Mock mockValue = new();
        _registry.Register("test", mockValue);

        Assert.That(mockValue.Id, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void GetValue_WithId_ReturnsCorrectValue()
    {
        Mock mockValue = new();
        _registry.Register("test", mockValue);

        IHasId result = _registry.GetValue(0);

        Assert.That(result, Is.EqualTo(mockValue));
    }

    [Test]
    public void GetValue_WithKey_ReturnsCorrectValue()
    {
        Mock mockValue = new();
        _registry.Register("test", mockValue);

        IHasId result = _registry.GetValue("test");

        Assert.That(result, Is.EqualTo(mockValue));
    }

    [Test]
    public void GetValue_WithInvalidId_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(() => _registry.GetValue(9999));
    }

    [Test]
    public void GetValue_WithInvalidKey_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(() => _registry.GetValue("invalid"));
    }*/
}