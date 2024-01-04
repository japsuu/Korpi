using BlockEngine.Client.Math;
using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace ClientTests;

[TestFixture]
public class CoordinateUtilsTests
{
    [Test]
    public void GetContainingChunkPos_ReturnsCorrectChunkPosition_ForPositiveVector3()
    {
        // Arrange
        Vector3 position = new(36, 74, 5);

        // Act
        Vector3i result = CoordinateUtils.WorldToChunk(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(32, 64, 0)));
    }


    [Test]
    public void GetContainingChunkPos_ReturnsCorrectChunkPosition_ForNegativeVector3()
    {
        // Arrange
        Vector3 position = new(-36, -74, -5);

        // Act
        Vector3i result = CoordinateUtils.WorldToChunk(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(-64, -96, -32)));
    }


    [Test]
    public void GetContainingChunkPos_ReturnsCorrectChunkPosition_ForVector3i()
    {
        // Arrange
        Vector3i position = new(36, 74, 5);

        // Act
        Vector3i result = CoordinateUtils.WorldToChunk(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(32, 64, 0)));
    }


    [Test]
    public void GetContainingColumnPos_ReturnsCorrectColumnPosition_ForVector3()
    {
        // Arrange
        Vector3 position = new(36, 74, 5);

        // Act
        Vector2i result = CoordinateUtils.WorldToColumn(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector2i(32, 0)));
    }


    [Test]
    public void GetContainingColumnPos_ReturnsCorrectColumnPosition_ForVector3i()
    {
        // Arrange
        Vector3i position = new(36, 74, 5);

        // Act
        Vector2i result = CoordinateUtils.WorldToColumn(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector2i(32, 0)));
    }


    [Test]
    public void GetChunkRelativePos_ReturnsCorrectRelativePosition()
    {
        // Arrange
        Vector3i position = new(36, 74, 5);

        // Act
        Vector3i result = CoordinateUtils.WorldToChunkRelative(position);

        // Assert
        Assert.That(result, Is.EqualTo(new Vector3i(4, 10, 5)));
    }
}