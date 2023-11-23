using BlockEngine.Framework.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

public class ChunkRenderer
{
    private readonly int _meshVBO;
    private readonly int _meshEBO;
    private readonly int _meshVAO;
    private readonly Matrix4 _modelMatrix;
    private readonly int _indicesCount;


    public ChunkRenderer(int meshVBO, int meshEBO, int meshVAO, int indicesCount, Matrix4 modelMatrix)
    {
        _meshVBO = meshVBO;
        _meshEBO = meshEBO;
        _meshVAO = meshVAO;
        _indicesCount = indicesCount;
        _modelMatrix = modelMatrix;
    }


    public void Draw(Shader chunkShader)
    {
        chunkShader.SetMatrix4("model", _modelMatrix);

        // Bind the VAO.
        GL.BindVertexArray(_meshVAO);

        // Draw.
        GL.DrawElements(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
}