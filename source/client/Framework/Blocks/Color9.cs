namespace BlockEngine.Client.Framework.Blocks;

/// <summary>
/// 9-bit color value.
/// Three bits for each color channel.
/// </summary>
public struct Color9
{
    public readonly int Value;
    
    /// <param name="r">Value in range 0-7 to use for red channel.</param>
    /// <param name="g">Value in range 0-7 to use for green channel.</param>
    /// <param name="b">Value in range 0-7 to use for blue channel.</param>
    public Color9(byte r, byte g, byte b)
    {
        Value = (r << 6) | (g << 3) | b;
    }
    
    
    public static Color9 White => new(7, 7, 7);
}