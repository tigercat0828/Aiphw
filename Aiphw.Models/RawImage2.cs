using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Aiphw.Models;

[SuppressMessage("Microsoft.Design", "CA1416:ValidatePlatformCompatibility")]
public class RawImage2 : IDisposable {
    private const int B = 0, G = 1, R = 2, A = 3;
    private Bitmap bitmap;
    public int Width => bitmap.Width;
    public int Height => bitmap.Height;

    public byte[] Pixels { get; private set; }
    public RawImage2() { }
    public RawImage2(string filename) {
        string extension = Path.GetExtension(filename);
        if (extension == ".ppm") {
            bitmap = PpmReadWriter.ReadPPM(filename);
        }
        else {
            bitmap = new Bitmap(filename);
        }
        Bitmap2Pixels();
    }
    public RawImage2(RawImage2 other) {
        bitmap = other.bitmap.Clone() as Bitmap;
        Pixels = other.Pixels.ToArray();
    }
    public RawImage2(int width, int height) {
        bitmap = new Bitmap(width, height);
        Bitmap2Pixels();
    }
    public RawImage2(byte[] pixels, int width, int height) {
        Pixels = pixels.ToArray();
        bitmap = new Bitmap(width, height);
        Pixels2Bitmap();
    }
    public Bitmap ToBitmap() {
        return new Bitmap(bitmap);
    }
    public void FinishEdit() {
        Pixels2Bitmap();
    }
    public void SetPixel(int x, int y, byte[] color) {
        int index = (y * Width + x) * 4;
        Pixels[index + B] = color[B];
        Pixels[index + G] = color[G];
        Pixels[index + R] = color[R];
        Pixels[index + A] = color[A];
    }
    public void SetPixel(int x, int y, byte value) {
        int index = (y * Width + x) * 4;
        Pixels[index + B] = value;
        Pixels[index + G] = value;
        Pixels[index + R] = value;
        Pixels[index + A] = 255;
    }
    public byte[] GetPixel(int x, int y) {
        /// todo : optimize here
        int index = (y * Width + x) * 4;
        return new byte[] {
                Pixels[index + B],
                Pixels[index + G],
                Pixels[index + R],
                Pixels[index + A],
            };
    }

    public void SaveFile(string filename) {
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

    public void Dispose() {
        bitmap.Dispose();
    }

    private void Bitmap2Pixels() {
        BitmapData bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );
        Pixels = new byte[bitmapData.Stride * bitmap.Height];
        Marshal.Copy(bitmapData.Scan0, Pixels, 0, Pixels.Length);
        bitmap.UnlockBits(bitmapData);
    }
    private void Pixels2Bitmap() {
        BitmapData bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb
        );
        Marshal.Copy(Pixels, 0, bitmapData.Scan0, Pixels.Length);
        bitmap.UnlockBits(bitmapData);
    }

}