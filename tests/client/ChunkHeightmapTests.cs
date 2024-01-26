using Korpi.Client.World.Chunks;

namespace ClientTests;

[TestFixture]
public class ChunkHeightmapTests
{
    [Test]
    public void OnBlockPlaced_UpdatesHeightmapCorrectly()
    {
        ChunkHeightmap chunkHeightmap = new ChunkHeightmap();
        chunkHeightmap.OnBlockAdded(1, 2, 3);
        Assert.That(chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(2));
    }


    [Test]
    public void OnBlockPlaced_UpdatesHeightmapWithHighestBlock()
    {
        ChunkHeightmap chunkHeightmap = new ChunkHeightmap();
        chunkHeightmap.OnBlockAdded(1, 2, 3);
        chunkHeightmap.OnBlockAdded(1, 3, 3);
        Assert.That(chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(3));
    }


    [Test]
    public void OnBlockRemoved_UpdatesHeightmapCorrectly()
    {
        ChunkHeightmap chunkHeightmap = new ChunkHeightmap();
        chunkHeightmap.OnBlockAdded(1, 2, 3);
        chunkHeightmap.OnBlockAdded(1, 3, 3);
        chunkHeightmap.OnBlockRemoved(1, 3, 3);
        Assert.That(chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(2));
    }


    [Test]
    public void OnBlockRemoved_LeavesHeightmapUnchangedWhenNotHighestBlock()
    {
        ChunkHeightmap chunkHeightmap = new ChunkHeightmap();
        chunkHeightmap.OnBlockAdded(1, 2, 3);
        chunkHeightmap.OnBlockAdded(1, 3, 3);
        chunkHeightmap.OnBlockRemoved(1, 2, 3);
        Assert.That(chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(3));
    }


    [Test]
    public void GetHighestBlock_ReturnsNegativeOneWhenNoBlocksInColumn()
    {
        ChunkHeightmap chunkHeightmap = new ChunkHeightmap();
        Assert.That(chunkHeightmap.GetHighestBlock(1, 3), Is.EqualTo(-1));
    }
}