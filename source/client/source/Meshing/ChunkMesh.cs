using OpenTK.Mathematics;

namespace Korpi.Client.Meshing;

/// <summary>
/// Contains the mesh data for a chunk, and the chunk's position.
///
/// Opaque and transparent mesh data is stored in separate arrays.
/// </summary>
public class ChunkMesh
{
    /// <summary>
    /// The position of the chunk in the world.
    /// </summary>
    public readonly Vector3i ChunkPos;
    
    /// <summary>
    /// The vertex data for the opaque blocks.
    /// 2 uints per vertex, 4 vertices per quad.
    /// </summary>
    public readonly uint[] OpaqueVertexData;
    
    /// <summary>
    /// The index data for the opaque blocks.
    /// </summary>
    public readonly uint[] OpaqueIndices;
    
    /// <summary>
    /// The vertex data for the transparent blocks.
    /// 2 uints per vertex, 4 vertices per quad.
    /// </summary>
    public readonly uint[] TransparentVertexData;
    
    /// <summary>
    /// The index data for the transparent blocks.
    /// </summary>
    public readonly uint[] TransparentIndices;
    
    /// <summary>
    /// The level of detail of the mesh.
    /// </summary>
    public readonly int LodLevel;


    public ChunkMesh(Vector3i chunkPos, uint[] opaqueVertexData, uint[] opaqueIndices, uint[] transparentVertexData, uint[] transparentIndices, int lodLevel)
    {
        ChunkPos = chunkPos;
        
        OpaqueVertexData = opaqueVertexData;
        OpaqueIndices = opaqueIndices;
        
        TransparentVertexData = transparentVertexData;
        TransparentIndices = transparentIndices;
        LodLevel = lodLevel;
    }
}