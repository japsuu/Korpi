﻿using BlockEngine.Framework.Rendering.ImGuiWindows;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Represents the mesh of a single chunk.
/// Contains:
/// - The vertex data. We do not store the indices because we use glDrawArrays.
/// - The chunk position.
/// - The VAO.
/// - The VBO.
/// </summary>
public class ChunkMesh
{
    public readonly uint[] Vertices;
    public readonly uint[] Indices;

    public readonly Vector3i ChunkPos;
    
    public int VerticesCount => Vertices.Length;
    public int IndicesCount => Indices.Length;


    public ChunkMesh(Vector3i chunkPos, uint[] vertices, uint[] indices)
    {
        ChunkPos = chunkPos;
        Vertices = vertices;
        Indices = indices;
        
        Logger.Debug($"Created chunk mesh with {vertices.Length / 2} vertices and {indices.Length} indices.");
        for (int i = 0; i < Vertices.Length; i += 2)
        {
            uint positionIndex = Vertices[i] & 0xFFFF;
            uint x = (positionIndex >> 10) & 0x1F;
            uint y = (positionIndex >> 5) & 0x1F;
            uint z = positionIndex & 0x1F;
            DebugTextWindow.AddStaticText(new Vector3(x, y, z), $"({i / 2}) => [{x}, {y}, {z}]");
            // Logger.Debug($"Vertex {i} = {_vertices[i]}\t({x}, {y}, {z})");
        }

        for (int i = 0; i < Indices.Length; i++)
            Logger.Debug($"Index {i} = {Indices[i]}");
    }
}