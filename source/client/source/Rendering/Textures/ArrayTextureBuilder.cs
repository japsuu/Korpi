using Korpi.Client.Exceptions;
using Korpi.Client.Modding.Blocks;

namespace Korpi.Client.Rendering.Textures;

public class ArrayTextureBuilder
{
    private readonly List<string> _texPaths = new();
    private readonly Dictionary<string, ushort> _texPathToIndex = new();
    private static ushort nextTextureIndex;


    public ushort AddTexture(string texturePath)
    {
        if (nextTextureIndex >= Constants.MAX_SUPPORTED_TEXTURES)
            throw new IdOverflowException("Texture ID overflow. Too many textures registered.");
        
        if (_texPathToIndex.TryGetValue(texturePath, out ushort textureIndex))
            return textureIndex;

        textureIndex = nextTextureIndex;
        _texPathToIndex.Add(texturePath, textureIndex);
        _texPaths.Add(texturePath);
        nextTextureIndex++;
        return textureIndex;
    }
    
    
    public ushort[] AddTextures(string containingFolderPath, YamlBlockFaceTextureData textureData)
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
        return ArrayTexture.LoadFromFiles(_texPaths.ToArray(), texName);
    }
}