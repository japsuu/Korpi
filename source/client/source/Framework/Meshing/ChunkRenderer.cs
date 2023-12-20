using BlockEngine.Client.Framework.Rendering.Shaders;
using BlockEngine.Client.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Meshing;

public class ChunkRenderer : IDisposable
{
    private readonly int _meshVAO;
    private readonly int _meshVBO;
    private readonly int _meshEBO;
    private readonly Matrix4 _modelMatrix;

    private bool _isDisposed;

    public readonly int VerticesCount;
    public readonly int IndicesCount;

    public ChunkRenderer(uint[] vertexData, uint[] indexData, Vector3i chunkPos)
    {
        VerticesCount = vertexData.Length;
        IndicesCount = indexData.Length;
        _modelMatrix = Matrix4.CreateTranslation(chunkPos);

        _meshVBO = GL.GenBuffer();
        _meshVAO = GL.GenVertexArray();
        _meshEBO = GL.GenBuffer();
        GL.BindVertexArray(_meshVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _meshVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, VerticesCount * sizeof(uint), vertexData, BufferUsageHint.StaticDraw);

        GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _meshEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, IndicesCount * sizeof(uint), indexData, BufferUsageHint.StaticDraw);

        // Cleanup.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public void Draw(Shader chunkShader)
    {
        chunkShader.SetMatrix4("model", _modelMatrix);

        // Bind the VAO.
        GL.BindVertexArray(_meshVAO);

        // Draw.
        GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        GL.DeleteBuffer(_meshVBO);
        GL.DeleteBuffer(_meshEBO);
        GL.DeleteVertexArray(_meshVAO);

        _isDisposed = true;
    }

    ~ChunkRenderer()
    {
        if (_isDisposed == false)
        {
            Logger.LogWarning($"[{nameof(ChunkRenderer)}] GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
}