using BlockEngine.Client.Meshing;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Rendering.Chunks;

/// <summary>
/// Used to store and dynamically fetch chunk meshes/renderers.
/// </summary>
public static class ChunkRendererStorage
{
    private static readonly Dictionary<Vector3i, ChunkRenderer> GeneratedRenderers = new();


    public static int GeneratedRendererCount => GeneratedRenderers.Count;
    
    
    public static void AddOrUpdateChunkMesh(ChunkMesh mesh)
    {
        if (GeneratedRenderers.TryGetValue(mesh.ChunkPos, out ChunkRenderer? renderer))
        {
            renderer.UpdateMesh(mesh);
        }
        else
        {
            renderer = new ChunkRenderer(mesh);
            GeneratedRenderers.Add(mesh.ChunkPos, renderer);
        }
    }
    
    
    public static void RemoveChunkMesh(Vector3i chunkPos)
    {
        GeneratedRenderers.Remove(chunkPos, out ChunkRenderer? renderer);
        if (renderer is not null)
        {
            renderer.Dispose();
        }
    }
    
    
    public static bool TryGetRenderer(Vector3i chunkPos, out ChunkRenderer? renderer)
    {
        return GeneratedRenderers.TryGetValue(chunkPos, out renderer);
    }
    
    
    public static bool ContainsRenderer(Vector3i chunkPos)
    {
        return GeneratedRenderers.ContainsKey(chunkPos);
    }
}