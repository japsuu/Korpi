using System.Diagnostics;
using BlockEngine.Framework.Blocks.Serialization;
using BlockEngine.Framework.Blocks.Textures;
using BlockEngine.Framework.Rendering.Textures;

namespace BlockEngine.Framework.Registries;

public static class TextureRegistry
{
    public static ArrayTexture BlockArrayTexture { get; private set; } = null!;
    
    private static ArrayTextureBuilder? arrayTextureBuilder;
    
    
    public static void StartTextureRegistration()
    {
        arrayTextureBuilder = new ArrayTextureBuilder();
    }
    
    
    public static void FinishTextureRegistration()
    {
        Debug.Assert(arrayTextureBuilder != null, nameof(arrayTextureBuilder) + " != null");
        BlockArrayTexture = arrayTextureBuilder.Build();
    }
    
    
    public static BlockFaceTextureCollection RegisterBlockTextures(string containingFolderPath, BlockFaceTextureCollectionData dataFaceTextures)
    {
        if (arrayTextureBuilder == null)
            throw new InvalidOperationException("Texture registration not started.");
        
        BlockFaceTexture[] textures = new BlockFaceTexture[6];
        
        textures[0] = RegisterTexture(containingFolderPath, dataFaceTextures.Front);
        textures[1] = RegisterTexture(containingFolderPath, dataFaceTextures.Top);
        textures[2] = RegisterTexture(containingFolderPath, dataFaceTextures.Right);
        textures[3] = RegisterTexture(containingFolderPath, dataFaceTextures.Back);
        textures[4] = RegisterTexture(containingFolderPath, dataFaceTextures.Bottom);
        textures[5] = RegisterTexture(containingFolderPath, dataFaceTextures.Left);
        
        return new BlockFaceTextureCollection(textures);
    }


    private static BlockFaceTexture RegisterTexture(string containingFolderPath, BlockFaceTextureData textureData)
    {
        Debug.Assert(arrayTextureBuilder != null, nameof(arrayTextureBuilder) + " != null");
        ushort[] textureIndices = arrayTextureBuilder.AddTextures(containingFolderPath, textureData);

        return textureIndices.Length > 1 ?
            new AnimatedBlockFaceTexture(textureIndices, 1f) :
            new BlockFaceTexture(textureIndices[0]);
    }
}