using BlockEngine.Framework.Blocks;
using BlockEngine.Framework.Chunks;
using BlockEngine.Utils;
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
    public void ChunkColumn_Load_DoesNotLoadForNonZeroPosition()
    {
        ChunkColumn chunkColumn = new(new Vector2i(1, 1));

        chunkColumn.Load();

        Assert.IsNull(chunkColumn.GetChunk(0));
    }

    [Test]
    public void Chunk_SetBlockState_SetsMeshDirty()
    {
        Chunk chunk = new();

        chunk.SetBlockState(new Vector3i(0, 0, 0), BlockRegistry.Stone.GetDefaultState());

        Assert.IsTrue(chunk.IsMeshDirty);
    }

    [Test]
    public void Chunk_GetBlockState_ReturnsCorrectBlockState()
    {
        Chunk chunk = new();
        BlockState blockState = BlockRegistry.Stone.GetDefaultState();

        chunk.SetBlockState(new Vector3i(0, 0, 0), blockState);

        Assert.That(chunk.GetBlockState(0, 0, 0), Is.EqualTo(blockState));
    }
}