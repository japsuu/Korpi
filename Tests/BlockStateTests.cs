using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Meshing;

namespace Tests;

[TestFixture]
public class BlockStateTests
{
    [Test]
    public void Constructor_InitializesCorrectly()
    {
        Block block = new(BlockRenderType.Normal);
        BlockState blockState = new(block);

        Assert.Multiple(() =>
        {
            Assert.That(blockState.RenderType, Is.EqualTo(BlockRenderType.Normal));
            Assert.That(blockState.Data, Is.EqualTo(0));
            Assert.That(blockState.NeighbourMask, Is.EqualTo(0));
        });
    }

    [Test]
    public void SetData_UpdatesDataCorrectly()
    {
        BlockState blockState = new(new Block(BlockRenderType.Normal));
        blockState.SetData(123);

        Assert.That(blockState.Data, Is.EqualTo(123));
    }

    [Test]
    public void UpdateNeighborMask_UpdatesMaskCorrectly_WhenNeighborExists()
    {
        BlockState blockState = new(new Block(BlockRenderType.Normal));
        blockState.SetNeighborMask(BlockFace.XPositive, true);

        Assert.That(blockState.NeighbourMask, Is.EqualTo(1 << (int)BlockFace.XPositive));
    }

    [Test]
    public void UpdateNeighborMask_UpdatesMaskCorrectly_WhenNeighborDoesNotExist()
    {
        BlockState blockState = new(new Block(BlockRenderType.Normal));
        blockState.SetNeighborMask(BlockFace.XPositive, false);

        Assert.That(blockState.NeighbourMask, Is.EqualTo(0));
    }

    [Test]
    public void HasNeighbor_ReturnsCorrectValue_WhenNeighborExists()
    {
        BlockState blockState = new(new Block(BlockRenderType.Normal));
        blockState.SetNeighborMask(BlockFace.XPositive, true);

        Assert.That(blockState.HasNeighbor(BlockFace.XPositive), Is.True);
    }

    [Test]
    public void HasNeighbor_ReturnsCorrectValue_WhenNeighborDoesNotExist()
    {
        BlockState blockState = new(new Block(BlockRenderType.Normal));
        blockState.SetNeighborMask(BlockFace.XPositive, false);

        Assert.That(blockState.HasNeighbor(BlockFace.XPositive), Is.False);
    }

    [Test]
    public void GetRotation_ReturnsCorrectRotation()
    {
        BlockState blockState = new(new Block(BlockRenderType.Normal));
        blockState.SetRotation(BlockOrientation.South);
        
        Assert.That(blockState.GetRotation(), Is.EqualTo(BlockOrientation.South));
    }

    [Test]
    public void Equals_ReturnsTrue_WhenBlockStatesAreEqual()
    {
        BlockState blockState1 = new(new Block(BlockRenderType.Normal));
        BlockState blockState2 = new(new Block(BlockRenderType.Normal));

        Assert.That(blockState1, Is.EqualTo(blockState2));
    }

    [Test]
    public void Equals_ReturnsFalse_WhenBlockStatesAreNotEqual()
    {
        BlockState blockState1 = new(new Block(BlockRenderType.Normal));
        BlockState blockState2 = new(new Block(BlockRenderType.Transparent));

        Assert.That(blockState1, Is.Not.EqualTo(blockState2));
    }
}