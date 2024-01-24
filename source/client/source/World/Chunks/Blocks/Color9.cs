using System.Diagnostics;
using Korpi.Client.Utils;

namespace Korpi.Client.World.Chunks.Blocks;

/// <summary>
/// 9-bit RGB333 color value.
/// Three bits for each color channel.
/// </summary>
public readonly struct Color9
{
    /// <summary>
    /// The 9-bit RGB333 color value.
    /// Only the 9 least significant bits are used.
    /// </summary>
    public readonly int Packed;
    
    
    public static Color9 White => new(7, 7, 7);
    
    
    /// <param name="r">Value in range 0-7 to use for red channel.</param>
    /// <param name="g">Value in range 0-7 to use for green channel.</param>
    /// <param name="b">Value in range 0-7 to use for blue channel.</param>
    public Color9(int r, int g, int b)
    {
        Debug.Assert(r >= 0 && r <= 7, "r must be in range 0-7");
        Debug.Assert(g >= 0 && g <= 7, "g must be in range 0-7");
        Debug.Assert(b >= 0 && b <= 7, "b must be in range 0-7");
        Packed = (r << 6) | (g << 3) | b;
    }
    
    
    public void Decompose(out int r, out int g, out int b)
    {
        r = (Packed >> 6) & 0b111;
        g = (Packed >> 3) & 0b111;
        b = Packed & 0b111;
    }
    
    
    /// <summary>
    /// Performs a component-wise Max between two colors and returns the result.
    /// </summary>
    public static Color9 Max(Color9 a, Color9 b)
    {
        a.Decompose(out int aR, out int aG, out int aB);
        b.Decompose(out int bR, out int bG, out int bB);
        
        return new Color9(
            System.Math.Max(aR, bR),
            System.Math.Max(aG, bG),
            System.Math.Max(aB, bB)
        );
    }


    public override string ToString()
    {
        Decompose(out int r, out int g, out int b);
        return $"Color9({r}, {g}, {b}, packed = {StringUtils.AsBits(Packed, 9)})";
    }
}