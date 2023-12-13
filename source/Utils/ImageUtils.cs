namespace BlockEngine.Utils;

public static class ImageUtils
{
    public static byte[] FlipImageVertically(byte[] pixels, int width, int height)
    {
        byte[] flippedPixels = new byte[pixels.Length];
        const int bytesPerPixel = 4;
        int stride = width * bytesPerPixel;
        for (int y = 0; y < height; y++)
        {
            int srcOffset = y * stride;
            int dstOffset = (height - y - 1) * stride;
            Buffer.BlockCopy(pixels, srcOffset, flippedPixels, dstOffset, stride);
        }
        return flippedPixels;
    }
}