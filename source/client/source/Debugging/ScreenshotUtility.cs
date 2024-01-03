using BigGustave;
using BlockEngine.Client.Logging;
using OpenTK.Graphics.OpenGL4;
using Buffer = System.Buffer;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace BlockEngine.Client.Debugging;

public static class ScreenshotUtility
{
    public class FrameCapture
    {
        private readonly byte[] _pixels;
        private readonly int _width;
        private readonly int _height;
        private string _fileName;
        
        
        public FrameCapture(byte[] pixels, int width, int height)
        {
            _pixels = pixels;
            _width = width;
            _height = height;
            _fileName = $"screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
        }
        
        
        public string SaveAsPng(string folderPath, string? overrideName = null, bool renameOther = false, bool renameOtherFromCreatedDateTime = false)
        {
            if (!string.IsNullOrEmpty(overrideName))
                _fileName = overrideName;
            
            byte[] pngBytes = CreatePngBytes();
            
            string filePath = Path.Combine(folderPath, $"{_fileName}.png");

            // Check if file exists. If renameOther is true, rename the other file. Otherwise rename this file.
            if (renameOther && File.Exists(filePath))
            {
                string otherFilePath = filePath;
                int i = 0;
                while (File.Exists(otherFilePath))
                {
                    if (renameOtherFromCreatedDateTime)
                    {
                        // Get the existing file creation time.
                        DateTime existingFileCreatedDateTime = File.GetCreationTime(otherFilePath);
                        // Get the new file name.
                        if (i == 0)
                        {
                            otherFilePath = Path.Combine(folderPath, $"screenshot_{existingFileCreatedDateTime:yyyy-MM-dd_HH-mm-ss}.png");
                        }
                        else
                        {
                            otherFilePath = Path.Combine(folderPath, $"screenshot_{existingFileCreatedDateTime:yyyy-MM-dd_HH-mm-ss}_{i}.png");
                        }
                    }
                    else
                    {
                        otherFilePath = Path.Combine(folderPath, $"{_fileName}_{i}.png");
                        i++;
                    }
                }
                // Move the existing file to the new name.
                File.Move(filePath, otherFilePath);
            }
            else if (File.Exists(filePath))
            {
                int i = 0;
                while (File.Exists(filePath))
                {
                    filePath = Path.Combine(folderPath, $"{_fileName}_{i}.png");
                    i++;
                }
            }
            
            // Check if folder exists.
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            File.WriteAllBytes(filePath, pngBytes);
            
            string fullPath = Path.GetFullPath(filePath);
            Logger.Log($"Saved screenshot to {fullPath}");
            return filePath;
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
        
        // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
        // so we need to flip the image vertically.
        pixels = FlipImageVertically(pixels, width, height);
        
        return new FrameCapture(pixels, width, height);
    }


    private static byte[] FlipImageVertically(byte[] pixels, int width, int height)
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