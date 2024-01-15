namespace Korpi.Client.Rendering;

/// <summary>
/// The render pass of a mesh.
/// </summary>
public enum RenderPass
{
    /// <summary>
    /// Render opaque geometry.
    /// Opaque meshes are rendered first.
    /// </summary>
    Opaque,
    
    /// <summary>
    /// Render transparent geometry.
    /// Transparent meshes are rendered after opaque meshes.
    /// </summary>
    Transparent
}