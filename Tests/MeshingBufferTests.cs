// using BlockEngine.Framework.Blocks;
// using BlockEngine.Framework.Meshing;
// using OpenTK.Mathematics;
//
// namespace Tests;
//
// [TestFixture]
// public class MeshingBufferTests
// {
//     private MeshingBuffer _meshingBuffer = null!;
//
//     [SetUp]
//     public void SetUp()
//     {
//         _meshingBuffer = new MeshingBuffer();
//     }
//
//     [Test]
//     public void AddFace_IncrementsAddedFacesCount()
//     {
//         _meshingBuffer.Clear();
//         _meshingBuffer.AddFace(new Vector3i(1, 1, 1), BlockFace.XPositive, 1, new Color9(1, 1, 1), 1, 1);
//
//         Assert.That(_meshingBuffer.AddedFacesCount, Is.EqualTo(1));
//     }
//
//     [Test]
//     public void AddFace_ThrowsException_WhenTextureIndexOutOfRange()
//     {
//         Assert.Throws<ArgumentOutOfRangeException>(() => _meshingBuffer.AddFace(new Vector3i(1, 1, 1), BlockFace.XPositive, 5000, new Color9(1, 1, 1), 1, 1));
//     }
//
//     [Test]
//     public void CreateMesh_ReturnsChunkMesh_WithCorrectVerticesAndIndicesCount()
//     {
//         _meshingBuffer.Clear();
//         _meshingBuffer.AddFace(new Vector3i(1, 1, 1), BlockFace.XPositive, 1, new Color9(1, 1, 1), 1, 1);
//         ChunkRenderer chunkMesh = _meshingBuffer.CreateMesh(new Vector3i(0, 0, 0));
//         Assert.Multiple(() =>
//         {
//             Assert.That(chunkMesh.VerticesCount, Is.EqualTo(8));
//             Assert.That(chunkMesh.IndicesCount, Is.EqualTo(6));
//         });
//     }
//
//     [Test]
//     public void Clear_ResetsAddedFacesCount()
//     {
//         _meshingBuffer.AddFace(new Vector3i(1, 1, 1), BlockFace.XPositive, 1, new Color9(1, 1, 1), 1, 1);
//         _meshingBuffer.Clear();
//
//         Assert.That(_meshingBuffer.AddedFacesCount, Is.EqualTo(0));
//     }
// }