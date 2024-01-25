using Korpi.Client.Debugging;
using Korpi.Client.Logging;
using Korpi.Client.Meshing;
using Korpi.Client.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Chunks;

public class ChunkRenderer : IDisposable
{
    private readonly int _opaqueMeshVAO;
    private readonly int _opaqueMeshVBO;
    private readonly int _opaqueMeshEBO;
    private readonly int _transparentMeshVAO;
    private readonly int _transparentMeshVBO;
    private readonly int _transparentMeshEBO;

    private Matrix4 _modelMatrix;
    private bool _isDisposed;

    private int _opaqueIndicesCount;
    private int _transparentIndicesCount;


    public ChunkRenderer(ChunkMesh mesh)
    {
        _modelMatrix = Matrix4.CreateTranslation(mesh.ChunkPos);
        _opaqueIndicesCount = mesh.OpaqueIndices.Length;
        _transparentIndicesCount = mesh.TransparentIndices.Length;

        _opaqueMeshVBO = GL.GenBuffer();
        _opaqueMeshVAO = GL.GenVertexArray();
        _opaqueMeshEBO = GL.GenBuffer();

        BufferData(_opaqueMeshVAO, _opaqueMeshVBO, _opaqueMeshEBO, mesh.OpaqueVertexData, mesh.OpaqueIndices, true);

        _transparentMeshVBO = GL.GenBuffer();
        _transparentMeshVAO = GL.GenVertexArray();
        _transparentMeshEBO = GL.GenBuffer();
        BufferData(_transparentMeshVAO, _transparentMeshVBO, _transparentMeshEBO, mesh.TransparentVertexData, mesh.TransparentIndices, true);
    }


    public void UpdateMesh(ChunkMesh mesh)
    {
        _modelMatrix = Matrix4.CreateTranslation(mesh.ChunkPos);
        _opaqueIndicesCount = mesh.OpaqueIndices.Length;
        _transparentIndicesCount = mesh.TransparentIndices.Length;

        BufferData(_opaqueMeshVAO, _opaqueMeshVBO, _opaqueMeshEBO, mesh.OpaqueVertexData, mesh.OpaqueIndices, false);
        BufferData(_transparentMeshVAO, _transparentMeshVBO, _transparentMeshEBO, mesh.TransparentVertexData, mesh.TransparentIndices, false);
    }


    public void Draw(RenderPass pass)
    {
        switch (pass)
        {
            case RenderPass.Opaque:
                ShaderManager.BlockOpaqueCutoutShader.ModelMat.Set(_modelMatrix);
                GL.BindVertexArray(_opaqueMeshVAO);

                // Draw opaque faces.
                if (_opaqueIndicesCount > 0)
                {
                    GL.DrawElements(PrimitiveType.Triangles, _opaqueIndicesCount, DrawElementsType.UnsignedInt, 0);
                    DebugStats.RenderedTris += (ulong)_opaqueIndicesCount / 3;
                }

                break;
            case RenderPass.Transparent:
                ShaderManager.BlockTranslucentShader.ModelMat.Set(_modelMatrix);
                GL.BindVertexArray(_transparentMeshVAO);

                // Draw transparent faces.
                if (_transparentIndicesCount > 0)
                {
                    GL.DrawElements(PrimitiveType.Triangles, _transparentIndicesCount, DrawElementsType.UnsignedInt, 0);
                    DebugStats.RenderedTris += (ulong)_transparentIndicesCount / 3;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pass), pass, null);
        }

        GL.BindVertexArray(0);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    private static void BufferData(int opaqueMeshVAO, int opaqueMeshVBO, int opaqueMeshEBO, uint[] vertexData, uint[] indices, bool initialize)
    {
        GL.BindVertexArray(opaqueMeshVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, opaqueMeshVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(uint), vertexData, BufferUsageHint.StaticDraw);

        if (initialize)
        {
            GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);
        }

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, opaqueMeshEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        // Cleanup.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        GL.DeleteBuffer(_opaqueMeshVBO);
        GL.DeleteBuffer(_opaqueMeshEBO);
        GL.DeleteVertexArray(_opaqueMeshVAO);

        GL.DeleteBuffer(_transparentMeshVBO);
        GL.DeleteBuffer(_transparentMeshEBO);
        GL.DeleteVertexArray(_transparentMeshVAO);

        _isDisposed = true;
    }


    ~ChunkRenderer()
    {
        if (_isDisposed == false)
            Logger.LogWarning($"[{nameof(ChunkRenderer)}] GPU Resource leak! Did you forget to call Dispose()?");
    }
}