using BigGustave;
using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace BlockEngine.Framework.Debugging;

public static class Screenshotter
{
    public class FrameCapture
    {
        private readonly byte[] _pixels;
        private readonly int _width;
        private readonly int _height;
        
        
        public FrameCapture(byte[] pixels, int width, int height)
        {
            _pixels = pixels;
            _width = width;
            _height = height;
        }
        
        
        public void SaveAsPng(string folderPath)
        {
            byte[] pngBytes = CreatePngBytes();
            string timeDate = $"{DateTime.Now:HH-mm-ss}";
            
            string filePath = Path.Combine(folderPath, $"screenshot_{timeDate}.png");

            // Check if file exists.
            if (File.Exists(filePath))
            {
                int i = 0;
                while (File.Exists(filePath))
                {
                    filePath = Path.Combine(folderPath, $"screenshot_{timeDate}_{i}.png");
                    i++;
                }
            }
            
            // Check if folder exists.
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            File.WriteAllBytes(filePath, pngBytes);
            
            Logger.Log($"Saved screenshot to {filePath}");
        }
        
        
        private byte[] CreatePngBytes()
        {
            PngBuilder builder = PngBuilder.Create(_width, _height, false);
            
            for (int i = 0; i < _pixels.Length; i += 4)
            {
                Pixel pixel = new(_pixels[i], _pixels[i + 1], _pixels[i + 2]);
                builder.SetPixel(pixel, i / 4 % _width, i / 4 / _width);
            }

            PngBuilder.SaveOptions options = new();
            return builder.Save(options);
        }
    }
    
    
    public static FrameCapture CaptureFrame(int width, int height)
    {
        byte[] pixels = new byte[width * height * 4];

        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        
        return new FrameCapture(pixels, width, height);
    }
}