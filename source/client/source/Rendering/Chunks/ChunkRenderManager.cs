using Korpi.Client.Configuration;
using Korpi.Client.Meshing;

namespace Korpi.Client.Rendering.Chunks;

/// <summary>
/// Used to manage the rendering of a chunk.
/// </summary>
public class ChunkRenderManager
{
    private readonly ChunkRenderer?[] _renderers = new ChunkRenderer?[Constants.TERRAIN_LOD_LEVEL_COUNT];


    public static int GeneratedRendererCount { get; private set; }
    
    
    public void AddOrUpdateMesh(ChunkMesh mesh)
    {
        int lodLevel = mesh.LodLevel;
        if (_renderers[lodLevel] != null)
        {
            _renderers[lodLevel]!.UpdateMesh(mesh);
        }
        else
        {
            _renderers[lodLevel] = new ChunkRenderer(mesh);
            GeneratedRendererCount++;
        }
    }
    
    
    public void DeleteMesh()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i]?.Dispose();
            _renderers[i] = null;
        }

        GeneratedRendererCount--;
    }
    
    
    public void RenderMesh(RenderPass pass, int lodLevel)
    {
        if (lodLevel is < 0 or >= Constants.TERRAIN_LOD_LEVEL_COUNT)
            throw new ArgumentOutOfRangeException(nameof(lodLevel));
        
        _renderers[lodLevel]?.Draw(pass);
    }
}