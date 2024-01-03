using System.Diagnostics;

namespace BlockEngine.Client.World.Chunks.Blocks.Textures;

/// <summary>
/// Contains textures for all faces of a <see cref="Block"/>.
/// </summary>
public class BlockFaceTextureCollection
{
    private readonly IBlockFaceTexture[] _faceTextures;
    
    
    public BlockFaceTextureCollection(IBlockFaceTexture[] faceTextures)
    {
        Debug.Assert(faceTextures.Length == 6, "BlockTextures must contain exactly 6 textures");
        _faceTextures = faceTextures;
    }
    
    
    /// <returns>the global texture index of the texture for the given face</returns>
    public ushort GetTextureId(BlockFace face) => _faceTextures[(int)face].GetId();
}