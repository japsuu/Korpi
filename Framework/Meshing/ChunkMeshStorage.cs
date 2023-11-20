using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Used to store and dynamically fetch chunk meshes.
/// </summary>
public static class ChunkMeshStorage
{
    private static readonly Dictionary<Vector3i, ChunkMesh> _generatedMeshes = new();
    
    
    public static void AddMesh(Vector3i chunkPos, ChunkMesh mesh)
    {
        _generatedMeshes.Add(chunkPos, mesh);
    }
    
    
    public static void RemoveMesh(Vector3i chunkPos)
    {
        _generatedMeshes.Remove(chunkPos);
    }
    
    
    public static ChunkMesh? GetMesh(Vector3i chunkPos)
    {
        return _generatedMeshes.TryGetValue(chunkPos, out ChunkMesh? mesh) ? mesh : null;
    }
}