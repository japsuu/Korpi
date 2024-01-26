using System.Diagnostics;
using Korpi.Client.Configuration;
using OpenTK.Mathematics;

namespace Korpi.Client.World;

/// <summary>
/// Represents a blocks position inside a subchunk.
/// </summary>
public readonly struct SubChunkBlockPosition
{
    public readonly int Index;
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
  
    public Vector3i AsVec3I => new Vector3i(X, Y, Z);
  
  
    public SubChunkBlockPosition(int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(y >= 0 && y < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(z >= 0 && z < Constants.SUBCHUNK_SIDE_LENGTH);
        Index = x + (y << Constants.SUBCHUNK_SIDE_BITS) + (z << Constants.SUBCHUNK_SIDE_BITS * 2);
        X = x;
        Y = y;
        Z = z;
    }


    public SubChunkBlockPosition(Vector3i position)
    {
        Debug.Assert(position.X >= 0 && position.X < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(position.Y >= 0 && position.Y < Constants.SUBCHUNK_SIDE_LENGTH);
        Debug.Assert(position.Z >= 0 && position.Z < Constants.SUBCHUNK_SIDE_LENGTH);
        Index = position.X + (position.Y << Constants.SUBCHUNK_SIDE_BITS) + (position.Z << Constants.SUBCHUNK_SIDE_BITS * 2);
        X = position.X;
        Y = position.Y;
        Z = position.Z;
    }
}