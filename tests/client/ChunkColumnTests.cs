using BlockEngine;
using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Chunks;
using OpenTK.Mathematics;

namespace Tests;

public class ChunkColumnTests
{
    [Test]
    public void ChunkColumn_InitializesWithCorrectPosition()
    {
        Vector2i position = new(5, 5);
        ChunkColumn chunkColumn = new(position);

        Assert.That(chunkColumn.Position, Is.EqualTo(position));
    }

    [Test]
    public void ChunkColumn_ReadyToUnload_ReturnsTrue()
    {
        ChunkColumn chunkColumn = new(Vector2i.Zero);

        Assert.IsTrue(chunkColumn.ReadyToUnload());
    }

    [Test]
    public void ChunkColumn_GetChunkAtHeight_ReturnsNullForOutOfRange()
    {
        ChunkColumn chunkColumn = new(Vector2i.Zero);

        Assert.IsNull(chunkColumn.GetChunkAtHeight(-1));
        Assert.IsNull(chunkColumn.GetChunkAtHeight(Constants.CHUNK_COLUMN_HEIGHT_BLOCKS));
    }

    [Test]
    public void ChunkColumn_GetChunk_ReturnsNullForUninitializedChunk()
    {
        ChunkColumn chunkColumn = new(Vector2i.Zero);

        Assert.IsNull(chunkColumn.GetChunk(0));
    }

    [Test]
    public void ChunkColumn_Tick_DoesNotSetMeshDirtyWhenNoChunksAreDirty()
    {
        ChunkColumn chunkColumn = new(Vector2i.Zero);

        chunkColumn.Tick(1.0);

        Assert.IsFalse(chunkColumn.IsMeshDirty);
    }
    
    [Test]
    public void Chunk_GetBlockState_ReturnsCorrectBlockState()
    {
        Chunk chunk = new();
        Block block = new Block(1, "test", BlockRenderType.None, null);
        BlockState blockState = block.GetDefaultState();

        chunk.SetBlockState(new Vector3i(3, 3, 3), blockState);

        Assert.That(chunk.GetBlockState(3, 3, 3), Is.EqualTo(blockState));
    }
}