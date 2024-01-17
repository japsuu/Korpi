using Korpi.Client.Logging;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.Rendering.Textures;

/// <summary>
/// Collection of methods to bind textures to texture units statically (for the entire lifetime of the program).
/// </summary>
[Obsolete("No >:(")]
public static class StaticTextureBindings
{
    private static readonly Dictionary<TextureUnit, Texture> StaticallyBoundTextureUnits = new();
    
    
    public static bool IsTextureUnitStaticallyBound(TextureUnit textureUnit) => StaticallyBoundTextureUnits.ContainsKey(textureUnit);
    
    public static string GetTextureNameForUnit(TextureUnit unit) => StaticallyBoundTextureUnits[unit].Name;


    /// <summary>
    /// Binds a texture to a texture unit.
    /// No other texture can be bound to the same texture unit until <see cref="UnbindTexture"/> is called.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the texture is already statically bound</exception>
    public static void BindTexture(Texture texture, TextureUnit textureUnit)
    {
        if (IsTextureUnitStaticallyBound(textureUnit))
            throw new InvalidOperationException($"Texture unit {textureUnit} is already statically bound to {GetTextureNameForUnit(textureUnit)}.");
        
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(texture.TextureTarget, texture.Handle);
        StaticallyBoundTextureUnits.Add(textureUnit, texture);
        Logger.Log($"Bound texture '{texture.Name}' to texture unit {textureUnit}.");
    }
    
    
    public static void UnbindTexture(Texture texture, TextureUnit textureUnit)
    {
        if (!IsTextureUnitStaticallyBound(textureUnit))
            throw new InvalidOperationException($"Texture unit {textureUnit} is not statically bound.");
        
        if (StaticallyBoundTextureUnits[textureUnit].Handle != texture.Handle)
            throw new InvalidOperationException($"Texture unit {textureUnit} is not bound to texture '{texture.Name}', but to texture '{GetTextureNameForUnit(textureUnit)}'.");
        
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(texture.TextureTarget, 0);
        StaticallyBoundTextureUnits.Remove(textureUnit);
        Logger.Log($"Unbound texture '{texture.Name}' from texture unit {textureUnit}.");
    }
}