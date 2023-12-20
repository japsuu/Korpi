namespace BlockEngine.Client.Utils;

public static class MathUtils
{
    /// <summary>
    /// Too inaccurate to be used for ray-casting, should be only used for structure generation.
    /// </summary>
    public static IEnumerable<(int x, int y, int z)> BresenhamLine3D(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;
        int dz = z2 - z1;

        int dx1 = 0, dy1 = 0, dz1 = 0, dx2 = 0, dy2 = 0, dz2 = 0;

        dx1 = dx switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dx1
        };

        dy1 = dy switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dy1
        };

        dz1 = dz switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dz1
        };

        dx2 = dx switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dx2
        };

        int longest = Math.Abs(dx);
        int shortest = Math.Min(Math.Abs(dy), Math.Abs(dz));

        if (longest <= shortest)
        {
            longest = Math.Max(Math.Abs(dy), Math.Abs(dz));
            shortest = Math.Min(Math.Abs(dy), Math.Abs(dz));

            if (dy < 0) dy2 = -1;
            else if (dy > 0) dy2 = 1;

            if (dz < 0) dz2 = -1;
            else if (dz > 0) dz2 = 1;

            dx2 = 0;
        }

        int numerator = longest >> 1;

        for (int i = 0; i <= longest; i++)
        {
            yield return (x1, y1, z1);

            numerator += shortest;

            if (numerator >= longest)
            {
                numerator -= longest;
                x1 += dx1;
                y1 += dy1;
                z1 += dz1;
            }
            else
            {
                x1 += dx2;
                y1 += dy2;
                z1 += dz2;
            }
        }
    }


    public static float Lerp(float min, float max, float factor)
    {
        return min + (max - min) * factor;
    }
}