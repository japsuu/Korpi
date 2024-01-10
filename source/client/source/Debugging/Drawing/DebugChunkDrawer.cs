using Korpi.Client.Configuration;
using Korpi.Client.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Debugging.Drawing;

public static class DebugChunkDrawer
{
    private static float[] chunkVertices = null!;
    private static uint[] chunkIndices = null!;
    private static float[] columnVertices = null!;
    private static uint[] columnIndices = null!;
    
    private static int chunkVBO;
    private static int chunkEBO;
    private static int chunkVAO;
    private static int columnVBO;
    private static int columnEBO;
    private static int columnVAO;
    
    
    public static void Initialize()
    {
        GenerateChunkMesh();
        GenerateColumnMesh();
        
        InitializeChunkMesh();
        InitializeColumnMesh();
    }


    private static void InitializeChunkMesh()
    {
        chunkVBO = GL.GenBuffer();
        chunkVAO = GL.GenVertexArray();
        GL.BindVertexArray(chunkVAO);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, chunkVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, chunkVertices.Length * sizeof(float), chunkVertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        chunkEBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, chunkEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, chunkIndices.Length * sizeof(uint), chunkIndices, BufferUsageHint.StaticDraw);
    }


    private static void InitializeColumnMesh()
    {
        columnVBO = GL.GenBuffer();
        columnVAO = GL.GenVertexArray();
        GL.BindVertexArray(columnVAO);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, columnVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, columnVertices.Length * sizeof(float), columnVertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        columnEBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, columnEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, columnIndices.Length * sizeof(uint), columnIndices, BufferUsageHint.StaticDraw);
    }


    public static void DrawChunkBorders(Vector3i pos)
    {
        ShaderManager.DebugShader.Use();
        
        Matrix4 modelMatrix = Matrix4.CreateTranslation(pos);
        ShaderManager.DebugShader.SetMatrix4("model", modelMatrix);
        
        GL.BindVertexArray(chunkVAO);
        GL.DrawElements(PrimitiveType.LineStrip, chunkIndices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }


    public static void DrawChunkColumnBorders(Vector2i pos)
    {
        ShaderManager.DebugShader.Use();
        
        Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(pos.X, 0, pos.Y));
        ShaderManager.DebugShader.SetMatrix4("model", modelMatrix);
        
        GL.BindVertexArray(columnVAO);
        GL.DrawElements(PrimitiveType.LineStrip, columnIndices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
    
    
    public static void Dispose()
    {
        GL.DeleteBuffer(chunkVBO);
        GL.DeleteBuffer(chunkEBO);
        GL.DeleteVertexArray(chunkVAO);
        GL.DeleteBuffer(columnVBO);
        GL.DeleteBuffer(columnEBO);
        GL.DeleteVertexArray(columnVAO);
        
        chunkVertices = null!;
        chunkIndices = null!;
        columnVertices = null!;
        columnIndices = null!;
    }


    private static void GenerateChunkMesh()
    {
        // Generate a line mesh to be drawn as LineStrip.
        List<float> vertices = new();
        List<uint> indices = new();
        int indexBufferIndex = 0;
        for (int y = 0; y < Constants.CHUNK_SIDE_LENGTH + 1; y++)
        {
            // Adding the four vertices of the current line.
            vertices.Add(0);
            vertices.Add(y);
            vertices.Add(0);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
            vertices.Add(y);
            vertices.Add(0);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
            vertices.Add(y);
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(0);
            vertices.Add(y);
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
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

            if (y == Constants.CHUNK_SIDE_LENGTH)
            {
                indices.Add(corner1);
                indices.Add(corner2);
                indices.Add(corner3);
                indices.Add(corner4);
                continue;
            }
            
            // Adding the Z- columns of the current line.
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
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
            for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
            {
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(y + 1);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
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
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
            {
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y + 1);
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
            }
            indices.Add(corner4);
            
            // Adding the X- columns of the current line.
            for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
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
        chunkVertices = vertices.ToArray();
        chunkIndices = indices.ToArray();
    }


    private static void GenerateColumnMesh()
    {
        // Generate a line mesh to be drawn as LineStrip.
        List<float> vertices = new();
        List<uint> indices = new();
        int indexBufferIndex = 0;
        for (int y = 0; y < Constants.CHUNK_COLUMN_HEIGHT_BLOCKS + 1; y++)
        {
            // Adding the four vertices of the current line.
            vertices.Add(0);
            vertices.Add(y);
            vertices.Add(0);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
            vertices.Add(y);
            vertices.Add(0);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
            vertices.Add(y);
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
            vertices.Add(1);
            vertices.Add(0);
            vertices.Add(0);
            
            vertices.Add(0);
            vertices.Add(y);
            vertices.Add(Constants.CHUNK_SIDE_LENGTH);
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

            if (y == Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            {
                indices.Add(corner1);
                indices.Add(corner2);
                indices.Add(corner3);
                indices.Add(corner4);
                continue;
            }
            
            // Adding the Z- columns of the current line.
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
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
            for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
            {
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(y + 1);
                vertices.Add(z);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
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
            for (int x = 0; x < Constants.CHUNK_SIDE_LENGTH; x++)
            {
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y + 1);
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(Constants.CHUNK_SIDE_LENGTH);
                vertices.Add(1);
                vertices.Add(0);
                vertices.Add(0);
                
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
                indices.Add((uint) indexBufferIndex++);
            }
            indices.Add(corner4);
            
            // Adding the X- columns of the current line.
            for (int z = 0; z < Constants.CHUNK_SIDE_LENGTH; z++)
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
        columnVertices = vertices.ToArray();
        columnIndices = indices.ToArray();
    }
}