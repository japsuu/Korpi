using Korpi.Client.Debugging;
using Korpi.Client.Logging;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Window;
using Korpi.Client.World;
using OpenTK.Graphics.OpenGL4;

namespace Korpi.Client.Rendering;

public class GameWorldRenderer : IDisposable
{
    private static readonly float[] ZeroFiller = { 0.0f, 0.0f, 0.0f, 0.0f };
    private static readonly float[] OneFiller = { 1.0f, 1.0f, 1.0f, 1.0f };
    
    private readonly GameWorld _world;
    private readonly int _opaqueFbo;
    private readonly int _opaqueTexture;
    private readonly int _depthTexture;
    private readonly int _transparentFbo;
    private readonly int _accumTexture;
    private readonly int _revealTexture;
    private readonly ScreenQuad _screenQuad;


    public GameWorldRenderer(GameWorld world)
    {
        _world = world;
        _screenQuad = new ScreenQuad();
        
        // Setup framebuffers
        _opaqueFbo = GL.GenFramebuffer();
        _transparentFbo = GL.GenFramebuffer();
        
        // Set up attachments for opaque framebuffer
        // ----------------------------------------------------
        
        // Opaque texture
        _opaqueTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _opaqueTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, GameClient.WindowWidth, GameClient.WindowHeight, 0, PixelFormat.Rgba, PixelType.HalfFloat, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        
        // Depth texture
        _depthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _depthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, GameClient.WindowWidth, GameClient.WindowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _opaqueFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _opaqueTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthTexture, 0);
        
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            Logger.LogError("Failed to create opaque framebuffer");
            throw new Exception("Failed to create opaque framebuffer");
        }
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        // Set up attachments for transparent framebuffer
        // ----------------------------------------------------
        
        // Accumulation texture
        _accumTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _accumTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, GameClient.WindowWidth, GameClient.WindowHeight, 0, PixelFormat.Rgba, PixelType.HalfFloat, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        
        // Revealage texture
        _revealTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _revealTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, GameClient.WindowWidth, GameClient.WindowHeight, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _transparentFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _accumTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, _revealTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthTexture, 0);   // Opaque framebuffer's depth texture
        
        // Set draw buffers
        DrawBuffersEnum[] transparentDrawBuffers = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
        GL.DrawBuffers(2, transparentDrawBuffers);
        
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            Logger.LogError("Failed to create transparent framebuffer");
            throw new Exception("Failed to create transparent framebuffer");
        }
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }


    public void Draw()
    {
        DebugStats.RenderedTris = 0;
        
        DrawChunksOpaquePass();

        DrawChunksTransparentPass();
        
        DrawChunksCompositePass();
        
        DrawToBackbuffer();

#if DEBUG
        RegionManager.DrawDebugBorders();
#endif
        
        _world.EntityManager.Draw();
    }


    private void DrawChunksOpaquePass()
    {
        GL.Enable(EnableCap.CullFace);      // Cull backfaces
        GL.Enable(EnableCap.DepthTest);     // Enable depth testing
        GL.DepthFunc(DepthFunction.Less);   // Draw fragments that are closer to the camera
        GL.DepthMask(true);             // Enable writing to the depth buffer
        GL.Disable(EnableCap.Blend);        // Disable blending
        GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        
        // Bind opaque framebuffer to render solid objects
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _opaqueFbo);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        ShaderManager.ShaderBlockOpaque.Use();
        
        _world.RegionManager.DrawChunks(RenderPass.Opaque);
    }


    private void DrawChunksTransparentPass()
    {
        GL.Disable(EnableCap.CullFace);     // Do not cull backfaces
        GL.DepthMask(false);            // Disable writing to the depth buffer
        GL.Enable(EnableCap.Blend);         // Enable blending
        GL.BlendFunc(0, BlendingFactorSrc.One, BlendingFactorDest.One);
        GL.BlendFunc(1, BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcColor);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        
        // Bind transparent framebuffer to render translucent objects
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _transparentFbo);
        GL.ClearBuffer(ClearBuffer.Color, 0, ZeroFiller);
        GL.ClearBuffer(ClearBuffer.Color, 1, OneFiller);
        
        ShaderManager.ShaderBlockTranslucent.Use();
        
        _world.RegionManager.DrawChunks(RenderPass.Transparent);
    }


    private void DrawChunksCompositePass()
    {
        GL.DepthFunc(DepthFunction.Always);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        // Bind opaque framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _opaqueFbo);
        
        // Use composite shader
        ShaderManager.ShaderComposite.Use();
        
        // Draw screen quad
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _accumTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _revealTexture);
        _screenQuad.Draw();
    }


    private void DrawToBackbuffer()
    {
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(true);             // Enable depth writes so GL.Clear won't ignore clearing the depth buffer
        GL.Disable(EnableCap.Blend);
        
        // Bind backbuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        
        // Use screen shader
        ShaderManager.ShaderScreen.Use();
        
        // Draw final screen quad
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _opaqueTexture);
        _screenQuad.Draw();
    }


    public void Dispose()
    {
        GL.DeleteFramebuffer(_opaqueFbo);
        GL.DeleteFramebuffer(_transparentFbo);
        GL.DeleteTexture(_opaqueTexture);
        GL.DeleteTexture(_depthTexture);
        GL.DeleteTexture(_accumTexture);
        GL.DeleteTexture(_revealTexture);
        _screenQuad.Dispose();
    }
}


internal class ScreenQuad : IDisposable
{
    private static readonly float[] QuadVertices =
    {
        // positions		// uv
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
        1.0f, -1.0f, 0.0f,  1.0f, 0.0f,
        1.0f, 1.0f, 0.0f,   1.0f, 1.0f,

        1.0f, 1.0f, 0.0f,   1.0f, 1.0f,
        -1.0f, 1.0f, 0.0f,  0.0f, 1.0f,
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f
    };

    private readonly int _vao;
    private readonly int _vbo;


    public ScreenQuad()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, QuadVertices.Length * sizeof(float), QuadVertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

        GL.BindVertexArray(0);
    }
    
    
    public void Draw()
    {
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
    }
    
    
    private void ReleaseUnmanagedResources()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
    }


    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            // Release managed resources here
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    ~ScreenQuad()
    {
        Dispose(false);
    }
}