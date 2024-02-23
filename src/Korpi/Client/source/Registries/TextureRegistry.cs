using System.Diagnostics;
using Korpi.Client.Blocks.Textures;
using Korpi.Client.Modding.Blocks;
using Korpi.Client.Rendering.Textures;
using Korpi.Client.Utils;
using KorpiEngine.Core.Logging;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.Registries;

public static class TextureRegistry
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(TextureRegistry));
    
    public static Texture2DArray BlockArrayTexture { get; private set; } = null!;
    
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
        BlockArrayTexture.Bind(TextureUnit.Texture15);
        Logger.Info($"Bound block texture array to texture unit {TextureUnit.Texture15}.");
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