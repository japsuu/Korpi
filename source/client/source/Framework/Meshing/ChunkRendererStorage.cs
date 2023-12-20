using BlockEngine.Client.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

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
    
    
    public static void InvalidateRenderer(Vector3i chunkPos)
    {
        RemoveRenderer(chunkPos);
        Logger.LogWarning($"Removed renderer for chunk at {chunkPos}. TODO: Use glBufferSubdata to replace/update the allocated buffer/memory, rather than allocating new memory.");
    }
    
    
    public static void RemoveRenderer(Vector3i chunkPos)
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