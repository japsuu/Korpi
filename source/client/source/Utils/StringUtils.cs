namespace BlockEngine.Client.Utils;

public static class StringUtils
{
    public static string AsBits(int value, int bitCount)
    {
        return Convert.ToString(value, 2).PadLeft(bitCount, '0');
    }
}