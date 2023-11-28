using BlockEngine.Framework.Blocks.Serialization;
using BlockEngine.Framework.Exceptions;
using BlockEngine.Framework.Rendering.Textures;
using BlockEngine.Utils;

namespace BlockEngine.Framework.Registries;

public class ArrayTextureBuilder
{
    private readonly List<string> _addedTexturePaths = new();
    private readonly Dictionary<string, ushort> _addedTextures = new();
    private static ushort nextTextureIndex;


    private ushort AddTexture(string texturePath)
    {
        if (nextTextureIndex >= Constants.MAX_SUPPORTED_TEXTURES)
            throw new IdOverflowException("Texture ID overflow. Too many textures registered.");
        
        if (_addedTextures.TryGetValue(texturePath, out ushort textureIndex))
            return textureIndex;

        textureIndex = nextTextureIndex;
        _addedTextures.Add(texturePath, textureIndex);
        _addedTexturePaths.Add(texturePath);
        nextTextureIndex++;
        return textureIndex;
    }
    
    
    public ushort[] AddTextures(string containingFolderPath, BlockFaceTextureData textureData)
    {
        List<ushort> textureIndices = new();
        foreach (string texturePath in textureData.Textures)
        {
            ushort textureIndex = AddTexture(Path.Combine(containingFolderPath, texturePath));
            textureIndices.Add(textureIndex);
        }
        return textureIndices.ToArray();
    }
    
    
    public ArrayTexture Build(string texName)
    {
        return ArrayTexture.LoadFromFiles(_addedTexturePaths.ToArray(), texName);
    }
}