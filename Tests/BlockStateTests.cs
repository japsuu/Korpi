using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Meshing;

namespace Tests;

[TestFixture]
public class BlockStateTests
{
    [Test]
    public void Constructor_InitializesCorrectly()
    {
        Block block = new(6, BlockVisibility.Opaque);
        BlockState blockState = new(block);

        Assert.That(blockState.Id, Is.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(blockState.Visibility, Is.EqualTo(BlockVisibility.Opaque));
            Assert.That(blockState.Data, Is.EqualTo(0));
            Assert.That(blockState.NeighbourMask, Is.EqualTo(0));
        });
    }

    [Test]
    public void SetData_UpdatesDataCorrectly()
    {
        BlockState blockState = new(new Block(0, BlockVisibility.Opaque));
        blockState.SetData(123);

        Assert.That(blockState.Data, Is.EqualTo(123));
    }

    [Test]
    public void UpdateNeighborMask_UpdatesMaskCorrectly_WhenNeighborExists()
    {
        BlockState blockState = new(new Block(0, BlockVisibility.Opaque));
        blockState.SetNeighborMask(BlockFaceNormal.XPositive, true);

        Assert.That(blockState.NeighbourMask, Is.EqualTo(1 << (int)BlockFaceNormal.XPositive));
    }

    [Test]
    public void UpdateNeighborMask_UpdatesMaskCorrectly_WhenNeighborDoesNotExist()
    {
        BlockState blockState = new(new Block(0, BlockVisibility.Opaque));
        blockState.SetNeighborMask(BlockFaceNormal.XPositive, false);

        Assert.That(blockState.NeighbourMask, Is.EqualTo(0));
    }

    [Test]
    public void HasNeighbor_ReturnsCorrectValue_WhenNeighborExists()
    {
        BlockState blockState = new(new Block(0, BlockVisibility.Opaque));
        blockState.SetNeighborMask(BlockFaceNormal.XPositive, true);

        Assert.That(blockState.HasNeighbor(BlockFaceNormal.XPositive), Is.True);
    }

    [Test]
    public void HasNeighbor_ReturnsCorrectValue_WhenNeighborDoesNotExist()
    {
        BlockState blockState = new(new Block(0, BlockVisibility.Opaque));
        blockState.SetNeighborMask(BlockFaceNormal.XPositive, false);

        Assert.That(blockState.HasNeighbor(BlockFaceNormal.XPositive), Is.False);
    }

    [Test]
    public void GetRotation_ReturnsCorrectRotation()
    {
        BlockState blockState = new(new Block(0, BlockVisibility.Opaque));
        blockState.SetRotation(Orientation.South);
        
        Assert.That(blockState.GetRotation(), Is.EqualTo(Orientation.South));
    }

    [Test]
    public void Equals_ReturnsTrue_WhenBlockStatesAreEqual()
    {
        BlockState blockState1 = new(new Block(0, BlockVisibility.Opaque));
        BlockState blockState2 = new(new Block(0, BlockVisibility.Opaque));

        Assert.That(blockState1, Is.EqualTo(blockState2));
    }

    [Test]
    public void Equals_ReturnsFalse_WhenBlockStatesAreNotEqual()
    {
        BlockState blockState1 = new(new Block(0, BlockVisibility.Opaque));
        BlockState blockState2 = new(new Block(1, BlockVisibility.Opaque));

        Assert.That(blockState1, Is.Not.EqualTo(blockState2));
    }
}