using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Chunks;
using BlockEngine.Framework.Meshing;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace Tests;

public class ChunkTests
{
    [Test]
    public void SetBlockState_UpdatesBlockStateAndSetsMeshDirty()
    {
        // Arrange
        Chunk chunk = new();
        Vector3i position = new(1, 1, 1);
        BlockState blockState = new();

        // Act
        chunk.SetBlockState(position, blockState);

        // Assert
        Assert.IsTrue(chunk.IsMeshDirty);
    }

    [Test]
    public void GetBlockState_ReturnsCorrectBlockState()
    {
        // Arrange
        Chunk chunk = new();
        Vector3i position = new(1, 1, 1);
        BlockState blockState = new();
        chunk.SetBlockState(position, blockState);

        // Act
        BlockState result = chunk.GetBlockState(position.X, position.Y, position.Z);

        // Assert
        Assert.That(result, Is.EqualTo(blockState));
    }

    [Test]
    public void CacheMeshingData_FillsCacheWithCorrectData()
    {
        // Arrange
        Chunk chunk = new();
        MeshingDataCache meshingDataCache = new(Constants.CHUNK_SIZE);

        // Act
        Block block1 = new Block(BlockRenderType.Normal);
        block1.AssignId(0);
        chunk.SetBlockState(new Vector3i(0, 0, 0), new BlockState(block1));
        Block block2 = new Block(BlockRenderType.Normal);
        block2.AssignId(3);
        chunk.SetBlockState(new Vector3i(3, 3, 3), new BlockState(block2));
        chunk.CacheMeshingData(meshingDataCache);

        // Assert
        // Assert that the cache is filled with the correct data.
        Assert.Multiple(() =>
        {
            Assert.That(meshingDataCache.GetData(0, 0, 0).Id, Is.EqualTo(0));
            Assert.That(meshingDataCache.GetData(1, 1, 1).Id, Is.EqualTo(1));
        });
    }

    [Test]
    public void CacheMeshingData_WithPosition_FillsCacheWithCorrectData()
    {
        // Arrange
        Chunk chunk = new();
        MeshingDataCache meshingDataCache = new(Constants.CHUNK_SIZE);

        // Act
        Block block1 = new Block(BlockRenderType.Normal);
        block1.AssignId(0);
        chunk.SetBlockState(new Vector3i(0, 0, 0), new BlockState(block1));
        Block block2 = new Block(BlockRenderType.Normal);
        block2.AssignId(2);
        chunk.SetBlockState(new Vector3i(16, 0, 16), new BlockState(block2));
        chunk.CacheMeshingData(meshingDataCache, NeighbouringChunkPosition.FaceUp);

        // Assert
        // Assert that the cache is filled with the correct data.
        Assert.Multiple(() =>
        {
            Assert.That(meshingDataCache.GetData(0, 0, 0).Id, Is.EqualTo(0));
            Assert.That(meshingDataCache.GetData(17, meshingDataCache.BorderBlockIndex, 17).Id, Is.EqualTo(2));
        });
    }
}