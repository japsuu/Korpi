using Korpi.Client.Blocks;

namespace ClientTests.Blocks;

[TestFixture]
public class BlockStateTests
{
    [Test]
    public void SetFaceVisibility_SetsVisibilityCorrectly_WhenFaceShouldBeVisible()
    {
        Block block = new Block(1, "test", BlockRenderType.Opaque, null);
        BlockState blockState = new BlockState(block);
        blockState.SetFaceVisibility(BlockFace.XPositive, true);
        Assert.That(blockState.IsFaceVisible(BlockFace.XPositive), Is.True);
    }


    [Test]
    public void SetFaceVisibility_SetsVisibilityCorrectly_WhenFaceShouldNotBeVisible()
    {
        Block block = new Block(1, "test", BlockRenderType.Opaque, null);
        BlockState blockState = new BlockState(block);
        blockState.SetFaceVisibility(BlockFace.XPositive, false);
        Assert.That(blockState.IsFaceVisible(BlockFace.XPositive), Is.False);
    }


    [Test]
    public void SetFaceVisibility_UpdatesInvisibleFacesCorrectly_WhenMultipleFacesAreSet()
    {
        Block block = new Block(1, "test", BlockRenderType.Opaque, null);
        BlockState blockState = new BlockState(block);
        blockState.SetFaceVisibility(BlockFace.XPositive, true);
        blockState.SetFaceVisibility(BlockFace.XNegative, false);
        Assert.That(blockState.IsFaceVisible(BlockFace.XPositive), Is.True);
        Assert.That(blockState.IsFaceVisible(BlockFace.XNegative), Is.False);
    }


    [Test]
    public void SetFaceVisibility_DoesNotAffectOtherFaces_WhenOneFaceIsSet()
    {
        Block block = new Block(1, "test", BlockRenderType.Opaque, null);
        BlockState blockState = new BlockState(block);
        blockState.SetFaceVisibility(BlockFace.XPositive, true);
        Assert.That(blockState.IsFaceVisible(BlockFace.XPositive), Is.True);
    }
}