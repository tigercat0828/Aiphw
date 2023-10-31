using System.Drawing;

namespace Aiphw.Models;
public static class PpmReadWriter {
    private const int B = 0;
    private const int G = 1;
    private const int R = 2;
    private const int A = 3;
    public static void WritePPM(string filename, byte[] pixels, int width, int height) {
        using (StreamWriter writer = new StreamWriter(filename)) {
            // Write the PPM header
            writer.WriteLine("P3");                 // P6 format for binary PPM
            writer.WriteLine($"{width} {height}");  // Width, height
            writer.WriteLine("255");                // Maximum color value

            for (int i = 0; i < pixels.Length; i += 4) {
                byte b = pixels[i + B];
                byte g = pixels[i + G];
                byte r = pixels[i + R];
                writer.WriteLine($"{r,3} {g,3} {b,3}");
            }
        }
    }
    public static void WritePPM(string filename, uint[] pixels, int width, int height) {
        using (StreamWriter writer = new StreamWriter(filename)) {
            // Write the PPM header
            writer.WriteLine("P3");                 // P6 format for binary PPM
            writer.WriteLine($"{width} {height}");  // Width, height
            writer.WriteLine("255");                // Maximum color value

            for (int i = 0; i < pixels.Length; i++) {
                uint pixel = pixels[i];
                byte b = (byte)((pixel >> 0) & 0xFF);
                byte g = (byte)((pixel >> 8) & 0xFF);
                byte r = (byte)((pixel >> 16) & 0xFF);
                writer.WriteLine($"{r,3} {g,3} {b,3}");
            }
        }
    }
    public static Bitmap ReadPPM(string filename) {
        try {
            using (StreamReader reader = new StreamReader(filename)) {
                // Read and validate the PPM format (magic number)
                string format = reader.ReadLine()?.Trim();
                if (format != "P3") {
                    Console.WriteLine("Invalid PPM format. Only P3 (plain text) is supported.");
                    return null;
                }

                // Read and parse the width and height
                string[] dimensions = reader.ReadLine()?.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (dimensions == null || dimensions.Length != 2 || !int.TryParse(dimensions[0], out int width) || !int.TryParse(dimensions[1], out int height) || width <= 0 || height <= 0) {
                    Console.WriteLine("Invalid image dimensions.");
                    return null;
                }

                // Read and parse the maximum color value (usually 255)
                string maxColorValue = reader.ReadLine()?.Trim();
                int maxValue;
                if (!int.TryParse(maxColorValue, out maxValue) || maxValue <= 0 || maxValue > 255) {
                    Console.WriteLine("Invalid maximum color value.");
                    return null;
                }

                // Read and parse the pixel data
                byte[] pixelData = new byte[width * height * 3]; // Assuming RGB format
                int pixelIndex = 0;

                while (!reader.EndOfStream) {
                    string line = reader.ReadLine()?.Trim();
                    string[] pixelTokens = line?.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pixelTokens == null || pixelTokens.Length != 3 || !byte.TryParse(pixelTokens[0], out byte red) || !byte.TryParse(pixelTokens[1], out byte green) || !byte.TryParse(pixelTokens[2], out byte blue)) {
                        Console.WriteLine("Invalid pixel color values.");
                        return null;
                    }

                    pixelData[pixelIndex++] = red;
                    pixelData[pixelIndex++] = green;
                    pixelData[pixelIndex++] = blue;
                }

                // Create a Bitmap from the pixel data
                Bitmap bitmap = new Bitmap(width, height);

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        int pixelOffset = (y * width + x) * 3;
                        Color pixelColor = Color.FromArgb(pixelData[pixelOffset], pixelData[pixelOffset + 1], pixelData[pixelOffset + 2]);
                        bitmap.SetPixel(x, y, pixelColor);
                    }
                }

                return bitmap;
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error reading P3 PPM file: {ex.Message}");
            return null;
        }
    }
}
