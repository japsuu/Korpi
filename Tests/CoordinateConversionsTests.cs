using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace Tests;

[TestFixture]
public class CoordinateConversionsTests
{
    [Test]
    public void GetContainingChunkPos_ReturnsCorrectChunkPosition_ForPositiveVector3()
    {
        // Arrange
        Vector3 position = new(36, 74, 5);

        // Act
        Vector3i result = CoordinateConversions.GetContainingChunkPos(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(32, 64, 0)));
    }


    [Test]
    public void GetContainingChunkPos_ReturnsCorrectChunkPosition_ForNegativeVector3()
    {
        // Arrange
        Vector3 position = new(-36, -74, -5);

        // Act
        Vector3i result = CoordinateConversions.GetContainingChunkPos(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(-64, -128, -32)));
    }


    [Test]
    public void GetContainingChunkPos_ReturnsCorrectChunkPosition_ForVector3i()
    {
        // Arrange
        Vector3i position = new(36, 74, 5);

        // Act
        Vector3i result = CoordinateConversions.GetContainingChunkPos(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(32, 64, 0)));
    }


    [Test]
    public void GetContainingColumnPos_ReturnsCorrectColumnPosition_ForVector3()
    {
        // Arrange
        Vector3 position = new(36, 74, 5);

        // Act
        Vector2i result = CoordinateConversions.GetContainingColumnPos(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector2i(32, 0)));
    }


    [Test]
    public void GetContainingColumnPos_ReturnsCorrectColumnPosition_ForVector3i()
    {
        // Arrange
        Vector3i position = new(36, 74, 5);

        // Act
        Vector2i result = CoordinateConversions.GetContainingColumnPos(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector2i(32, 0)));
    }


    [Test]
    public void GetChunkRelativePos_ReturnsCorrectRelativePosition()
    {
        // Arrange
        Vector3i position = new(36, 74, 5);

        // Act
        Vector3i result = CoordinateConversions.GetChunkRelativePos(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(4, 10, 5)));
    }
}