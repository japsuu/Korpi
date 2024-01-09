using Korpi.Client.Registries;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.Rendering.Textures;

public abstract class Texture : IHasName, IDisposable
{
    public readonly int Handle;

    public string Name { get; }
    
    public abstract TextureTarget TextureTarget { get; }
    
    
    protected Texture(int glHandle, string name)
    {
        Handle = glHandle;
        Name = name;
    }


    // Activate texture
    // Multiple textures can be bound, if your shader needs more than just one.
    // If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
    // The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.
    [Obsolete("Use BindStatic instead.")]
    public void Bind(TextureUnit unit)
    {
        if (StaticTextureBindings.IsTextureUnitStaticallyBound(unit))
            throw new InvalidOperationException($"Texture unit {unit} is already statically bound to {StaticTextureBindings.GetTextureNameForUnit(unit)}.");
        
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget, Handle);
    }
    
    
    public void BindStatic(TextureUnit unit)
    {
        StaticTextureBindings.BindTexture(this, unit);
    }
    
    
    private void ReleaseUnmanagedResources()
    {
        GL.DeleteTexture(Handle);
    }
    
    
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~Texture()
    {
        ReleaseUnmanagedResources();
    }
}