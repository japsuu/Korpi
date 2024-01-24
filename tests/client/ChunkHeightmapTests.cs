using Korpi.Client.World.Chunks;

namespace ClientTests;

[TestFixture]
public class ChunkHeightmapTests
{
    private ChunkHeightmap _chunkHeightmap = null!;


    [SetUp]
    public void SetUp()
    {
        _chunkHeightmap = new ChunkHeightmap();
    }


    [Test]
    public void OnBlockPlaced_UpdatesHeightmapCorrectly()
    {
        _chunkHeightmap.OnBlockAdded(1, 2, 3);
        Assert.That(_chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(2));
    }


    [Test]
    public void OnBlockPlaced_UpdatesHeightmapWithHighestBlock()
    {
        _chunkHeightmap.OnBlockAdded(1, 2, 3);
        _chunkHeightmap.OnBlockAdded(1, 3, 3);
        Assert.That(_chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(3));
    }


    [Test]
    public void OnBlockRemoved_UpdatesHeightmapCorrectly()
    {
        _chunkHeightmap.OnBlockAdded(1, 2, 3);
        _chunkHeightmap.OnBlockAdded(1, 3, 3);
        _chunkHeightmap.OnBlockRemoved(1, 3, 3);
        Assert.That(_chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(2));
    }


    [Test]
    public void OnBlockRemoved_LeavesHeightmapUnchangedWhenNotHighestBlock()
    {
        _chunkHeightmap.OnBlockAdded(1, 2, 3);
        _chunkHeightmap.OnBlockAdded(1, 3, 3);
        _chunkHeightmap.OnBlockRemoved(1, 2, 3);
        Assert.That(_chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(3));
    }


    [Test]
    public void GetHighestBlock_ReturnsNegativeOneWhenNoBlocksInColumn()
    {
        Assert.That(_chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(-1));
    }
}