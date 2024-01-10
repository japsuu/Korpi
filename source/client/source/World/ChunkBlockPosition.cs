using System.Diagnostics;
using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.World;

/// <summary>
/// Represents a blocks position inside a chunk.
/// </summary>
public readonly struct ChunkBlockPosition
{
    public readonly int Index;
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
  
    public Vector3i AsVec3I => new Vector3i(X, Y, Z);
  
  
    public ChunkBlockPosition(int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Constants.CHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.CHUNK_SIDE_LENGTH);
        Debug.Assert(z >= 0 && z < Constants.CHUNK_SIDE_LENGTH);
        Index = x + (y << Constants.CHUNK_SIDE_BITS) + (z << Constants.CHUNK_SIDE_BITS_DOUBLED);
        X = x;
        Y = y;
        Z = z;
    }


    public ChunkBlockPosition(Vector3i position)
    {
        Debug.Assert(position.X >= 0 && position.X < Constants.CHUNK_SIDE_LENGTH);
        Debug.Assert(position.Y >= 0 && position.Y < Constants.CHUNK_SIDE_LENGTH);
        Debug.Assert(position.Z >= 0 && position.Z < Constants.CHUNK_SIDE_LENGTH);
        Index = position.X + (position.Y << Constants.CHUNK_SIDE_BITS) + (position.Z << Constants.CHUNK_SIDE_BITS_DOUBLED);
        X = position.X;
        Y = position.Y;
        Z = position.Z;
    }
}