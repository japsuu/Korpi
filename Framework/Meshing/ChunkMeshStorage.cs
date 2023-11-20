using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Used to store and dynamically fetch chunk meshes.
/// </summary>
public static class ChunkMeshStorage
{
    private static readonly Dictionary<Vector3i, ChunkMesh> _generatedMeshes = new();


    public static int GeneratedMeshCount { get; private set; }
    
    
    public static void AddMesh(Vector3i chunkPos, ChunkMesh mesh)
    {
        Logger.Debug($"Adding generated mesh for chunk at {chunkPos}");
        _generatedMeshes.Add(chunkPos, mesh);
        GeneratedMeshCount++;
    }
    
    
    public static void RemoveMesh(Vector3i chunkPos)
    {
        _generatedMeshes.Remove(chunkPos);
        GeneratedMeshCount--;
    }
    
    
    public static ChunkMesh? GetMesh(Vector3i chunkPos)
    {
        return _generatedMeshes.TryGetValue(chunkPos, out ChunkMesh? mesh) ? mesh : null;
    }
    
    
    public static bool ContainsMesh(Vector3i chunkPos)
    {
        return _generatedMeshes.ContainsKey(chunkPos);
    }
}