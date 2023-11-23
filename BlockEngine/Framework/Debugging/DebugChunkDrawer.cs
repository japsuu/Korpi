using BlockEngine.Framework.Rendering.Shaders;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Debugging;

public static class DebugChunkDrawer
{
    // Contains the 24 vertices (6 faces) of a cube. Extends from 0,0,0 to Constants.CHUNK_SIZE,Constants.CHUNK_SIZE,Constants.CHUNK_SIZE.
    private static float[] ChunkVertices = {
        Constants.CHUNK_SIZE, 0, Constants.CHUNK_SIZE, 0, 0, 1,
        Constants.CHUNK_SIZE, 0, 0, 0, 0, 1,
        Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, 0, 1, 0, 0,
        Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, 1, 0, 0,
        
        0, 0, 0, 0, 0, 1,
        0, 0, Constants.CHUNK_SIZE, 0, 0, 1,
        0, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, 1, 0, 0,
        0, Constants.CHUNK_SIZE, 0, 1, 0, 0,
    };
    
    private static uint[] ChunkIndices = {
        // X+ face
        0, 1, 3, 0,
        2, 1, 3, 2,
        
        // Y+ face
        2, 3, 7, 2,
        6, 7, 3, 6,
        
        // Z+ face
        3, 5, 6, 0,
        3, 5, 0, 5,
        
        // X- face
        6, 4, 5, 7,
        4, 6, 7, 4,
        
        // Y- face
        5, 1, 4, 0,
        1, 5, 0, 1,
        
        // Z- face
        2, 4, 1, 7,
        2, 4, 7, 6
    };
    
    private static int chunkVBO;
    private static int chunkEBO;
    private static int chunkVAO;
    
    
    public static void Initialize()
    {
        // Generate a line mesh to be drawn as LineStrip.
        // The mesh should be CHUNK_SIZE*CHUNK_SIZE*CHUNK_SIZE, and have the borders of the block grid.
        List<float> vertices = new();
        List<uint> indices = new();
        int indexBufferIndex = 0;
        for (int y = 0; y < Constants.CHUNK_SIZE; y++)
        {
            // Adding the four vertices of the current line.
            vertices.Add(0);
            vertices.Add(y);
            vertices.Add(0);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(Constants.CHUNK_SIZE);
            vertices.Add(y);
            vertices.Add(0);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(Constants.CHUNK_SIZE);
            vertices.Add(y);
            vertices.Add(Constants.CHUNK_SIZE);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(0);
            vertices.Add(y);
            vertices.Add(Constants.CHUNK_SIZE);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            indices.Add((uint) indexBufferIndex++);
            indices.Add((uint) indexBufferIndex++);
            indices.Add((uint) indexBufferIndex++);
            indices.Add((uint) indexBufferIndex++);
            indices.Add((uint) indexBufferIndex - 4);
            
            uint corner1 = (uint) indexBufferIndex - 4;
            uint corner2 = (uint) indexBufferIndex - 3;
            uint corner3 = (uint) indexBufferIndex - 2;
            uint corner4 = (uint) indexBufferIndex - 1;
            
            // Adding the Z- columns of the current line.
            for (int x = 0; x < Constants.CHUNK_SIZE; x++)
            {
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(0);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y + 1);
                vertices.Add(0);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(0);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
            }
            indices.Add(corner2);
            
            // Adding the X+ columns of the current line.
            for (int z = 0; z < Constants.CHUNK_SIZE; z++)
            {
                vertices.Add(Constants.CHUNK_SIZE);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(Constants.CHUNK_SIZE);
                vertices.Add(y + 1);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(Constants.CHUNK_SIZE);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
            }
            indices.Add(corner3);
            
            // Adding the Z+ columns of the current line.
            for (int x = 0; x < Constants.CHUNK_SIZE; x++)
            {
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(Constants.CHUNK_SIZE);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y + 1);
                vertices.Add(Constants.CHUNK_SIZE);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(Constants.CHUNK_SIZE);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
            }
            indices.Add(corner4);
            
            // Adding the X- columns of the current line.
            for (int z = 0; z < Constants.CHUNK_SIZE; z++)
            {
                vertices.Add(0);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(0);
                vertices.Add(y + 1);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(0);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
            }
            indices.Add(corner1);
        }
        ChunkVertices = vertices.ToArray();
        ChunkIndices = indices.ToArray();
        
        chunkVBO = GL.GenBuffer();
        chunkVAO = GL.GenVertexArray();
        GL.BindVertexArray(chunkVAO);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, chunkVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, ChunkVertices.Length * sizeof(float), ChunkVertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        chunkEBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, chunkEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, ChunkIndices.Length * sizeof(uint), ChunkIndices, BufferUsageHint.StaticDraw);
    }
    
    
    public static void DrawChunkBorders(Vector3i pos)
    {
        ShaderManager.DebugShader.Use();
        
        Matrix4 modelMatrix = Matrix4.CreateTranslation(pos);
        ShaderManager.DebugShader.SetMatrix4("model", modelMatrix);
        
        GL.BindVertexArray(chunkVAO);
        GL.DrawElements(PrimitiveType.LineStrip, ChunkIndices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
    
    
    public static void Dispose()
    {
        GL.DeleteBuffer(chunkVBO);
        GL.DeleteBuffer(chunkEBO);
        GL.DeleteVertexArray(chunkVAO);
    }
}