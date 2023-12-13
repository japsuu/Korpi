using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Used to store and dynamically fetch chunk meshes/renderers.
/// </summary>
public static class ChunkRendererStorage
{
    private static readonly Dictionary<Vector3i, ChunkRenderer> GeneratedRenderers = new();


    public static int GeneratedRendererCount { get; private set; }
    
    
    public static void AddRenderer(Vector3i chunkPos, ChunkRenderer renderer)
    {
        GeneratedRenderers.Add(chunkPos, renderer);
        GeneratedRendererCount++;
    }
    
    
    public static void RemoveRenderer(Vector3i chunkPos)
    {
        GeneratedRenderers.Remove(chunkPos);
        GeneratedRendererCount--;
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