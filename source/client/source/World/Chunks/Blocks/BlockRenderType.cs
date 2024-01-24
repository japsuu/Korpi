namespace Korpi.Client.World.Chunks.Blocks;

/// <summary>
/// Type of rendering of a <see cref="Block"/>.
/// </summary>
public enum BlockRenderType
{
    /// <summary>
    /// The block is not rendered.
    /// </summary>
    None,
    
    /// <summary>
    /// The block is opaque.
    /// </summary>
    Opaque,
    
    /// <summary>
    /// The block texture is alpha clipped.
    /// </summary>
    AlphaClip,
    
    /// <summary>
    /// The block is transparent.
    /// </summary>
    Transparent,
}