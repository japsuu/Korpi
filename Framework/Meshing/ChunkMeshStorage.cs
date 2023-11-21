using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Used to store and dynamically fetch chunk meshes.
/// </summary>
public static class ChunkMeshStorage
{
    private static readonly Dictionary<Vector3i, ChunkMesh> GeneratedMeshes = new();


    public static int GeneratedMeshCount { get; private set; }
    
    
    public static void AddMesh(Vector3i chunkPos, ChunkMesh mesh)
    {
        // Logger.Debug($"Adding generated mesh for chunk at {chunkPos}");
        GeneratedMeshes.Add(chunkPos, mesh);
        GeneratedMeshCount++;
    }
    
    
    public static void RemoveMesh(Vector3i chunkPos)
    {
        GeneratedMeshes.Remove(chunkPos);
        GeneratedMeshCount--;
    }
    
    
    public static bool TryGetMesh(Vector3i chunkPos, out ChunkMesh? mesh)
    {
        return GeneratedMeshes.TryGetValue(chunkPos, out mesh);
    }
    
    
    public static bool ContainsMesh(Vector3i chunkPos)
    {
        return GeneratedMeshes.ContainsKey(chunkPos);
    }
}