//#define LOAD_SLOW_CODE

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;

namespace Aiphw.Models;

public static class ImageProcessing {

    const int B = 0, G = 8, R = 16, A = 24;
    const double RAD2DEG = 180.0 / Math.PI;
    #region DEBUG
    public static void PrintPixelRgbaImage(RawImage image) {

        int Width = image.Width;
        int Height = image.Height;
        Console.WriteLine($"Width = {Width}");
        Console.WriteLine($"Height = {Height}");
        Console.WriteLine($"Pixel count = {image.Length / 4}");

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                int index = y * Width + x;
                uint b = image[index] >> B & 0xFF;
                uint g = image[index] >> G & 0xFF;
                uint r = image[index] >> R & 0xFF;
                uint a = image[index] >> A & 0xFF;
                Console.Write($"[{r,3} {g,3} {b,3} {a,3} ], ");
            }
            Console.WriteLine();
        }
    }

    public static void PrintPixelHexImage(RawImage image) {
        int Width = image.Width;
        int Height = image.Height;
        Console.WriteLine($"Width = {Width}");
        Console.WriteLine($"Height = {Height}");
        Console.WriteLine($"Pixel count = {image.Pixels.Length / 4}");
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                int index = y * Width + x;
                Console.Write($"{image[index], 12}, ");
            }
            Console.WriteLine();
        }
    }
    #endregion
    public static RawImage ConvolutionRGB(RawImage input, MaskKernel kernel) {
        int kernelSize = kernel.Size;
        int kernelOffset = kernelSize / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                float[] rgba = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

                for (int i = -kernelOffset; i <= kernelOffset; i++) {
                    for (int j = -kernelOffset; j <= kernelOffset; j++) {
                        int pX = Math.Clamp(x + i, 0, width - 1);
                        int pY = Math.Clamp(y + j, 0, height - 1);
                        int pIndex = (pY * width + pX);
                        float kernelValue = kernel[i + kernelOffset][j + kernelOffset];
                        rgba[0] += (input[pIndex] >> B & 0xFF) * kernelValue;
                        rgba[1] += (input[pIndex] >> G & 0xFF) * kernelValue;
                        rgba[2] += (input[pIndex] >> R & 0xFF) * kernelValue;
                    }
                }
                int index = y * width + x;
                uint b = (uint)Math.Clamp(rgba[0] / kernel.Scalar, 0, 255);
                uint g = (uint)Math.Clamp(rgba[1] / kernel.Scalar, 0, 255);
                uint r = (uint)Math.Clamp(rgba[2] / kernel.Scalar, 0, 255);
                output[index] = (b << B | g << G | r << R | 0xFF000000);
            }
        });
        return output;
    }
    public static RawImage ConvolutionGray(RawImage input, MaskKernel kernel) {
        int kernelOffset = kernel.Size / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                float gray = 0.0f;

                for (int i = -kernelOffset; i <= kernelOffset; i++) {
                    for (int j = -kernelOffset; j <= kernelOffset; j++) {
                        int pX = Math.Clamp(x + i, 0, width - 1);
                        int pY = Math.Clamp(y + j, 0, height - 1);
                        int pIndex = pY * width + pX;
                        float kernelValue = kernel[i + kernelOffset][j + kernelOffset];
                        gray += (input[pIndex] & 0xFF) * kernelValue;
                    }
                }
                int index = y * width + x;
                uint grayScale = (uint)Math.Clamp(gray / kernel.Scalar, 0, 255);
                output[index] = (grayScale << B | grayScale << G | grayScale << R | 0xFF000000);
            }
        });
        return output;
    }
    public static RawImage Smooth(RawImage input) {

        MaskKernel smoothMask = MaskKernel.LoadPreBuiltMask(DefaultMask.GaussianSmooth);
        RawImage garyScale = GrayScale(input);
        return ConvolutionGray(garyScale, smoothMask);

        return ConvolutionRGB(input, smoothMask);

        

    }
    public static RawImage Reverse(RawImage input) {

        RawImage output = new(input.Width, input.Height);

        int length = input.Length;
        Parallel.For(0, length, i => {
            uint b = 255 - (input[i] >> B & 0xFF);
            uint g = 255 - (input[i] >> G & 0xFF);
            uint r = 255 - (input[i] >> R & 0xFF);
            output[i] = (b << B | g << G | r << R | 0xFF000000);

        });
        return output;
    }
    public static RawImage EdgeDetection(RawImage input) {
        // Canny Edge Detection 
        // Step 1: Smooth
        RawImage grayscale = GrayScale(input);
        RawImage smooth = Smooth(grayscale);

        // Step 2: Calculate Gradient
        MaskKernel sobelXmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelX);
        MaskKernel sobelYmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelY);
        RawImage sobelXimage = ConvolutionGray(smooth, sobelXmask);
        RawImage sobelYimage = ConvolutionGray(smooth, sobelYmask);
        Func<uint, uint, uint> grad = (gx, gy) => (uint)Math.Clamp(Math.Sqrt(gx * gx + gy * gy), 0, 255);
        
        RawImage gradientData = OverlayCalculate(sobelXimage, sobelYimage, grad);
       
        return Reverse(gradientData);

    }
    public static RawImage GrayScale(RawImage input) {
        RawImage output = new(input.Width, input.Height);

        int numPx = input.Length;
        Parallel.For(0, numPx, i => {
            uint b = input[i] >> B & 0xFF;
            uint g = input[i] >> G & 0xFF;
            uint r = input[i] >> R & 0xFF;
            uint gray = (uint)((b + g + r) / 3.0f);
            //uint gray = (uint)(0.299 * r + 0.587 * g + 0.114 * b);
            output[i] = (gray << B | gray << G | gray << R | 0xFF000000);
        });

        return output;
    }
    public static RawImage RightRotate(RawImage input) {
        int width = input.Width;
        int height = input.Height;

        int newWidth = width;
        int newHeight = height;

        RawImage output = new(newHeight, newWidth);

        Parallel.For(0, height, j => {
            for (int i = 0; i < width; i++) {
                int inIndex = j * width + i;
                int x = newHeight - 1 - j;
                int y = i;
                int outIndex = y * newHeight + x;
                output[outIndex] = input[inIndex];
            }
        });
        return output;
    }
    public static RawImage LeftRotate(RawImage input) {
        int width = input.Width;
        int height = input.Height;

        int newWidth = height;
        int newHeight = width;

        RawImage output = new(newWidth, newHeight);

        Parallel.For(0, height, j => {
            for (int i = 0; i < width; i++) {
                int inIndex = j * width + i;
                int x = j;
                int y = width - 1 - i;
                int outIndex = y * newWidth + x;
                output[outIndex] = input[inIndex];
            }
        });

        return output;
    }
    public static RawImage SaltPepperNoise(RawImage input, out RawImage noise, int noiseValue) {
        Random random = new();
        RawImage output = new(input);

        RawImage noiseImage = new (input.Width, input.Height);
        uint black = 0xFF000000; // ARGB
        uint white = 0xFFFFFFFF;
        uint gray = 0xFF808080;
        int length = input.Length;
        noiseValue /= 2;

        Parallel.For(0, length, i => {
            int rnd = random.Next(100);
            if (rnd <= noiseValue) {
                output[i] = black;
                noiseImage[i] = black;
            }
            else if (rnd >= 100 - noiseValue) {
                output[i] = white;
                noiseImage[i] = white;
            }
            else {
                noiseImage[i] = gray;
            }
        });

        noise = noiseImage;
        return output;
    }
    public static RawImage GaussianNoise(RawImage input, out RawImage outNoise, float sigma) {
        Random varphi = new();
        Random gamma = new();

        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);
        RawImage noise = new(width, height);
        Parallel.For(0, height, y => {
            for (int x = 1; x < width; x += 2) {

                float sqrt = MathF.Sqrt(-2.0f * MathF.Log(gamma.NextSingle()));
                float trian = varphi.NextSingle() * 2.0f * MathF.PI;
                float z1 = sigma * MathF.Cos(trian) * sqrt;
                float z2 = sigma * MathF.Sin(trian) * sqrt;

                uint z1b = (uint)z1;
                uint z2b = (uint)z2;

                uint noise1 = (z1b << B | z1b << G | z1b << R | 0xFF000000);
                uint noise2 = (z2b << B | z2b << G | z2b << R | 0xFF000000);
                noise.SetPixel(x - 1, y, noise1);
                noise.SetPixel(x, y, noise2);

                uint p1 = input.GetPixel(x - 1, y);
                uint p2 = input.GetPixel(x, y);

                uint B1 = (uint)Math.Clamp((p1 >> B & 0xFF) + z1, 0, 255);
                uint G1 = (uint)Math.Clamp((p1 >> G & 0xFF) + z1, 0, 255);
                uint R1 = (uint)Math.Clamp((p1 >> R & 0xFF) + z1, 0, 255);

                uint B2 = (uint)Math.Clamp((p2 >> B & 0xFF) + z2, 0, 255);
                uint G2 = (uint)Math.Clamp((p2 >> G & 0xFF) + z2, 0, 255);
                uint R2 = (uint)Math.Clamp((p2 >> R & 0xFF) + z2, 0, 255);

                uint noised1 = (B1 << B | G1 << G | R1 << R | 0xFF000000);
                uint noised2 = (B2 << B | G2 << G | R2 << R | 0xFF000000);

                output.SetPixel(x - 1, y, noised1);
                output.SetPixel(x, y, noised2);
            }
        });

        outNoise = noise;
        return output;
    }
    public static RawImage DataToRawImage(RawImage image) {
        RawImage output = new(image.Width, image.Height);
        int length = output.Length;
        Parallel.For(0, length, i => {

            uint b = image[i] >> B & 0xFF;
            uint g = image[i] >> G & 0xFF;
            uint r = image[i] >> R & 0xFF;

            output[i] = (b << B | g << G | r << R | 0xFF000000);
        });

        return output;
    }
    public static RawImage OverlayCalculate(RawImage input1, RawImage input2, Func<uint, uint, uint> func) {

        if (input1.Width != input2.Width || input1.Height != input2.Height) {
            return null;
        }
        int width = input1.Width;
        int height = input1.Height;
        int length = input1.Length;
        RawImage output = new(width, height);

        Parallel.For(0, length, i => {

            uint b = func(input1[i] >> B & 0xFF, input2[i] >> B & 0xFF);
            uint g = func(input1[i] >> G & 0xFF, input2[i] >> G & 0xFF);
            uint r = func(input1[i] >> R & 0xFF, input2[i] >> R & 0xFF);

            output[i] = (b << B | g << G | r << R | 0xFF000000);
        });

        return output;
    }
#if LOAD_SLOW_CODE
    const int cB = 0, cG = 1, cR = 2, cA = 3;
    public static RawImage2 GrayScale2(RawImage2 input) {
        int width = input.Width;
        int height = input.Height;
        RawImage2 output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                int index = (y * width + x) * 4;
                byte gray = (byte)((input.Pixels[index + cB] + input.Pixels[index + cG] + input.Pixels[index + cR]) / 3);

                output.Pixels[index + cB] = gray;
                output.Pixels[index + cG] = gray;
                output.Pixels[index + cR] = gray;
                output.Pixels[index + cA] = 255;
            }
        });
        output.FinishEdit();
        return output;

    }
    public static RawImage2 Overlay2(RawImage2 input1, RawImage2 input2, Func<byte, byte, byte> func) {

        if (input1.Width != input2.Width || input1.Height != input2.Height) {
            return null;
        }
        int width = input1.Width;
        int height = input1.Height;

        RawImage2 output = new RawImage2(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                int index = (y * width + x) * 4;
                output.Pixels[index + cB] = func(input1.Pixels[index + cB], input2.Pixels[index + cB]);
                output.Pixels[index + cG] = func(input1.Pixels[index + cG], input2.Pixels[index + cG]);
                output.Pixels[index + cR] = func(input1.Pixels[index + cR], input2.Pixels[index + cR]);
                output.Pixels[index + cA] = 255;
            }
        });

        output.FinishEdit();
        return output;
    }
    public static RawImage2 Reverse2(RawImage2 input) {
        int width = input.Width;
        int height = input.Height;
        RawImage2 output = new(width, height);
        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                int index = (y * width + x) * 4;
                output.Pixels[index + cB] = (byte)(255 - input.Pixels[index + cB]);
                output.Pixels[index + cG] = (byte)(255 - input.Pixels[index + cG]);
                output.Pixels[index + cR] = (byte)(255 - input.Pixels[index + cR]);
                output.Pixels[index + cA] = 255;
            }
        });
        output.FinishEdit();
        return output;
    }
    public static RawImage2 Smooth2(RawImage2 input) {
        MaskKernel smoothMask = MaskKernel.LoadPreBuiltMask(DefaultMask.GaussianSmooth);
        return ConvolutionRGB2(input, smoothMask);
    }
    public static RawImage2 ConvolutionRGB2(RawImage2 input, MaskKernel kernel) {
        int kernelSize = kernel.Size;
        int kernelOffset = kernelSize / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage2 output = new RawImage2(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                float[] rgba = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

                for (int i = -kernelOffset; i <= kernelOffset; i++) {
                    for (int j = -kernelOffset; j <= kernelOffset; j++) {
                        int pixelX = Math.Clamp(x + i, 0, width - 1);
                        int pixelY = Math.Clamp(y + j, 0, height - 1);
                        int pixelIndex = (pixelY * width + pixelX) * 4;
                        float kernelValue = kernel[i + kernelOffset][j + kernelOffset];

                        rgba[cB] += input.Pixels[pixelIndex + cB] * kernelValue;
                        rgba[cG] += input.Pixels[pixelIndex + cG] * kernelValue;
                        rgba[cR] += input.Pixels[pixelIndex + cR] * kernelValue;
                        // rgba[A] += input.Pixels[pixelIndex + A] * 255;
                    }
                }

                int index = (y * width + x) * 4;
                output.Pixels[index + cB] = (byte)Math.Clamp(rgba[cB] / kernel.Scalar, 0, 255);
                output.Pixels[index + cG] = (byte)Math.Clamp(rgba[cG] / kernel.Scalar, 0, 255);
                output.Pixels[index + cR] = (byte)Math.Clamp(rgba[cR] / kernel.Scalar, 0, 255);
                output.Pixels[index + cA] = 255;
            }
        });

        output.FinishEdit();
        return output;
    }
    public static RawImage2 ConvolutionGray2(RawImage2 input, MaskKernel kernel) {
        int kernelSize = kernel.Size;
        int kernelOffset = kernelSize / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage2 output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                float[] rgba = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

                for (int i = -kernelOffset; i <= kernelOffset; i++) {
                    for (int j = -kernelOffset; j <= kernelOffset; j++) {
                        int X = Math.Clamp(x + i, 0, width - 1);
                        int Y = Math.Clamp(y + j, 0, height - 1);
                        int pIndex = (Y * width + X) * 4;
                        float kernelValue = kernel[i + kernelOffset][j + kernelOffset];

                        rgba[cB] += input.Pixels[pIndex + cB] * kernelValue;
                    }
                }
                int index = (y * width + x) * 4;
                output.Pixels[index + cB] = (byte)Math.Clamp(rgba[cB] / kernel.Scalar, 0, 255);
                output.Pixels[index + cG] = (byte)Math.Clamp(rgba[cB] / kernel.Scalar, 0, 255);
                output.Pixels[index + cR] = (byte)Math.Clamp(rgba[cB] / kernel.Scalar, 0, 255);
                output.Pixels[index + cA] = 255;
            }
        });

        output.FinishEdit();
        return output;
    }
    public static RawImage2 EdgeDetection2(RawImage2 input) {
        MaskKernel sobelXmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelX);
        MaskKernel sobelYmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelY);
        RawImage2 grayscale = GrayScale2(input);
        RawImage2 sobelXimage = ConvolutionGray2(input, sobelXmask);
        RawImage2 sobelYimage = ConvolutionGray2(input, sobelYmask);
        Func<byte, byte, byte> gradient = (a, b) => (byte)Math.Clamp(Math.Sqrt(a * a + b * b), 0, 255);
        RawImage2 edgeDetected = Overlay2(sobelYimage, sobelYimage, gradient);

        return Reverse2(edgeDetected);
    }

    public static void PrintPixelImage2(RawImage2 image) {
        const int B = 0, G = 1, R = 2, A = 3;
        int Width = image.Width;
        int Height = image.Height;
        Console.WriteLine($"Width = {Width}");
        Console.WriteLine($"Height = {Height}");
        Console.WriteLine($"Pixel count = {image.Pixels.Length / 4}");

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                int index = (y * Width + x) * 4;
                byte b = image.Pixels[index + B];
                byte g = image.Pixels[index + G];
                byte r = image.Pixels[index + R];
                byte a = image.Pixels[index + A];
                Console.Write($"[{r,3} {g,3} {b,3} {a,3} ], ");
            }
            Console.WriteLine();
        }
    }
    public static void PrintPixelStream2(RawImage2 image) {

        for (int i = 0; i < image.Pixels.Length; i += 4) {
            byte b = image.Pixels[i + cB];
            byte g = image.Pixels[i + cG];
            byte r = image.Pixels[i + cR];
            byte a = image.Pixels[i + cA];
            Console.WriteLine($"[{r,3} {g,3} {b,3} {a,3} ], ");
        }
    }
#endif
}
