using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

/// <summary>
/// Used to store and dynamically fetch chunk meshes/renderers.
/// </summary>
public static class ChunkRendererStorage
{
    private static readonly Dictionary<Vector3i, ChunkRenderer> GeneratedRenderers = new();


    public static int GeneratedRendererCount { get; private set; }
    
    
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
            GeneratedRendererCount++;
        }
    }
    
    
    public static void RemoveChunkMesh(Vector3i chunkPos)
    {
        GeneratedRenderers.Remove(chunkPos, out ChunkRenderer? renderer);
        if (renderer is not null)
        {
            renderer.Dispose();
            GeneratedRendererCount--;
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