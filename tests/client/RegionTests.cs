using Korpi;
using Korpi.Client;
using Korpi.Client.Configuration;
using Korpi.Client.Window;
using Korpi.Client.World.Regions;
using OpenTK.Mathematics;

namespace ClientTests;

public class RegionTests
{
    [Test]
    public void ChunkColumn_InitializesWithCorrectPosition()
    {
        Vector2i position = new(5, 5);
        Region region = new(position);

        Assert.That(region.Position, Is.EqualTo(position));
    }

    [Test]
    public void ChunkColumn_ReadyToUnload_ReturnsTrue()
    {
        Region region = new(Vector2i.Zero);

        Assert.IsTrue(region.ReadyToUnload());
    }

    [Test]
    public void ChunkColumn_GetChunkAtHeight_ReturnsNullForOutOfRange()
    {
        Region region = new(Vector2i.Zero);

        Assert.IsNull(region.GetChunkAtHeight(-1));
        Assert.IsNull(region.GetChunkAtHeight(Constants.CHUNK_COLUMN_HEIGHT_BLOCKS));
    }

    [Test]
    public void ChunkColumn_GetChunk_ReturnsNullForUninitializedChunk()
    {
        Region region = new(Vector2i.Zero);

        Assert.IsNull(region.GetChunk(0));
    }
}