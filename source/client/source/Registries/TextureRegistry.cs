using System.Diagnostics;
using BlockEngine.Client.Modding.Blocks;
using BlockEngine.Client.Rendering.Textures;
using BlockEngine.Client.Utils;
using BlockEngine.Client.World.Chunks.Blocks.Textures;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Client.Registries;

public static class TextureRegistry
{
    public static ArrayTexture BlockArrayTexture { get; private set; } = null!;
    
    private static ArrayTextureBuilder? arrayTextureBuilder;
    
    
    public static void StartTextureRegistration()
    {
        arrayTextureBuilder = new ArrayTextureBuilder();
        
        Debug.Assert(arrayTextureBuilder != null, nameof(arrayTextureBuilder) + " != null");
        arrayTextureBuilder.AddTexture(IoUtils.GetTexturePath("missing.png"));
    }
    
    
    public static void FinishTextureRegistration()
    {
        Debug.Assert(arrayTextureBuilder != null, nameof(arrayTextureBuilder) + " != null");
        BlockArrayTexture = arrayTextureBuilder.Build("BlockTextures");
        // Bind the BlockArrayTexture to texture unit 0.
        BlockArrayTexture.BindStatic(TextureUnit.Texture0);
    }
    
    
    public static BlockFaceTextureCollection RegisterBlockTextures(string containingFolderPath, YamlBlockTextureData dataFaceTextures)
    {
        if (arrayTextureBuilder == null)
            throw new InvalidOperationException("Texture registration not started.");
        
        IBlockFaceTexture[] textures = new IBlockFaceTexture[6];
        
        textures[0] = RegisterTexture(containingFolderPath, dataFaceTextures.Front);
        textures[1] = RegisterTexture(containingFolderPath, dataFaceTextures.Top);
        textures[2] = RegisterTexture(containingFolderPath, dataFaceTextures.Right);
        textures[3] = RegisterTexture(containingFolderPath, dataFaceTextures.Back);
        textures[4] = RegisterTexture(containingFolderPath, dataFaceTextures.Bottom);
        textures[5] = RegisterTexture(containingFolderPath, dataFaceTextures.Left);
        
        return new BlockFaceTextureCollection(textures);
    }


    private static IBlockFaceTexture RegisterTexture(string containingFolderPath, YamlBlockFaceTextureData textureData)
    {
        Debug.Assert(arrayTextureBuilder != null, nameof(arrayTextureBuilder) + " != null");
        ushort[] textureIndices = arrayTextureBuilder.AddTextures(containingFolderPath, textureData);

        return textureIndices.Length > 1 ?
            new AnimatedBlockFaceTexture(textureIndices, 1f) :
            new SingleBlockFaceTexture(textureIndices[0]);
    }
}