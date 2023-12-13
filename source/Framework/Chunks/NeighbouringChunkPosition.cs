namespace BlockEngine.Framework.Chunks;

public enum NeighbouringChunkPosition
{
    // Corners
    CornerNorthEastUp,
    CornerSouthEastUp,
    CornerSouthWestUp,
    CornerNorthWestUp,
    CornerNorthEastDown,
    CornerSouthEastDown,
    CornerSouthWestDown,
    CornerNorthWestDown,
        
    // Edges
    EdgeNorthUp,
    EdgeEastUp,
    EdgeSouthUp,
    EdgeWestUp,
    EdgeNorthDown,
    EdgeEastDown,
    EdgeSouthDown,
    EdgeWestDown,
    EdgeNorthEast,
    EdgeSouthEast,
    EdgeSouthWest,
    EdgeNorthWest,
        
    // Faces
    FaceNorth,
    FaceEast,
    FaceSouth,
    FaceWest,
    FaceUp,
    FaceDown
}