using Korpi.Client.Meshing;

namespace Korpi.Client.Rendering.Chunks;

/// <summary>
/// Used to manage the rendering of a chunk.
/// </summary>
public class ChunkRenderManager
{
    private ChunkRenderer? _renderer;
    
    
    public void AddOrUpdateMesh(ChunkMesh mesh)
    {
        if (_renderer != null)
        {
            _renderer.UpdateMesh(mesh);
        }
        else
        {
            _renderer = new ChunkRenderer(mesh);
        }
    }
    
    
    public void DeleteMesh()
    {
        _renderer?.Dispose();
        _renderer = null;
    }
    
    
    public void RenderMesh(RenderPass pass)
    {
        _renderer?.Draw(pass);
    }
}