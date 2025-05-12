using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Aiphw.Models;
[SuppressMessage("Microsoft.Design", "CA1416:ValidatePlatformCompatibility")]
public class RawImage {
    const int B = 0, G = 8, R = 16, A = 24;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Length => Width * Height;
    public uint[] Pixels;
    public uint this[int i] {
        get { return Pixels[i]; }
        set { Pixels[i] = value; }
    }
    public uint this[int i, int j] {
        get { return Pixels[j * Width + i]; }
        set { Pixels[j * Width + i] = value; }
    }
    public RawImage(string filename) {
        string extension = Path.GetExtension(filename);
        Bitmap bmp = null;
        if (extension == ".ppm") {
            bmp = PpmReadWriter.ReadPPM(filename);
        }
        else {
            bmp = new Bitmap(filename);
        }
        Pixels = GetPixels(bmp);
        Width = bmp.Width;
        Height = bmp.Height;
    }
    public RawImage(RawImage other) {
        Pixels = [.. other.Pixels];
        Width = other.Width;
        Height = other.Height;
    }
    public RawImage(int width, int height) {
        Width = width;
        Height = height;
        Pixels = new uint[width * height];
    }
    public RawImage(int width, int height, uint[] pixels) {
        Width = width;
        Height = height;
        Pixels = [.. pixels];
    }
    public void SetPixel(int x, int y, uint pixel) {
        int index = y * Width + x;
        Pixels[index] = pixel;
    }
    public void SetPixel(int x, int y, Color color) {
        uint pixelValue = ((uint)color.A << A) | ((uint)color.R << R) | ((uint)color.G << G) | color.B;
        SetPixel(x, y, pixelValue);
    }

    public uint GetPixel(int x, int y) {
        return Pixels[y * Width + x];
    }

    public Bitmap ToBitmap() {

        Bitmap bmp = new(Width, Height);

        Rectangle rect = new(0, 0, Width, Height);

        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        int numPixels = Width * Height;
        byte[] RGBA = new byte[numPixels * 4];

        Parallel.For(0, numPixels, i => {
            int index = i * 4;
            uint pixel = Pixels[i];
            RGBA[index + 0] = (byte)(pixel >> B & 0xFF);  // Blue
            RGBA[index + 1] = (byte)(pixel >> G & 0xFF);  // Green
            RGBA[index + 2] = (byte)(pixel >> R & 0xFF);  // Red
            RGBA[index + 3] = (byte)(pixel >> A & 0xFF);  // Alpha
        });
        IntPtr ptr = bmpData.Scan0;
        Marshal.Copy(RGBA, 0, ptr, numPixels * 4);
        bmp.UnlockBits(bmpData);

        return bmp;
    }
    public void SaveFile(string filename) {
        var bitmap = ToBitmap();
        string extension = Path.GetExtension(filename);
        switch (extension.ToLower()) {
            case ".jpg":
            case ".jpeg":
                bitmap.Save(filename, ImageFormat.Jpeg);
                break;
            case ".png":
                bitmap.Save(filename, ImageFormat.Png);
                break;
            case ".bmp":
                bitmap.Save(filename, ImageFormat.Bmp);
                break;
            case ".ppm":
                PpmReadWriter.WritePPM(filename, Pixels, Width, Height);
                break;
            default:
                Console.WriteLine("Unsupported file format.");
                break;
        }
    }
    public static uint[] GetPixels(Bitmap bitmap) {

        int width = bitmap.Width;
        int height = bitmap.Height;

        BitmapData bmpData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );

        int stride = bmpData.Stride;
        int byteDataSize = stride * height;

        byte[] pixelBytes = new byte[byteDataSize];
        Marshal.Copy(bmpData.Scan0, pixelBytes, 0, byteDataSize);
        bitmap.UnlockBits(bmpData);

        uint[] pixels = new uint[byteDataSize / 4];
        Buffer.BlockCopy(pixelBytes, 0, pixels, 0, byteDataSize);

        return pixels;
    }
}
