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
                Console.Write($"{image[index],12}, ");
            }
            Console.WriteLine();
        }
    }
    #endregion
    public static RawImage ConvolutionFullColor(RawImage input, MaskKernel kernel) {
        int kernelSize = kernel.Size;
        int kernelOffset = kernelSize / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                float[] rgba = [0.0f, 0.0f, 0.0f, 0.0f];

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
    public static RawImage ConvolutionGrayScale(RawImage input, MaskKernel kernel) {
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
    public static RawImage Smooth(RawImage input) {

        MaskKernel smoothMask = MaskKernel.LoadPreBuiltMask(DefaultMask.GaussianSmooth);
        RawImage garyScale = GrayScale(input);
        return ConvolutionFullColor(input, smoothMask);
        return ConvolutionGrayScale(garyScale, smoothMask);
    }

    public static RawImage EdgeDetection(RawImage input) {
        // Canny Edge Detection 
        // Step 1: Smooth
        RawImage grayscale = GrayScale(input);
        RawImage smooth = Smooth(grayscale);

        // Step 2: Calculate Gradient
        MaskKernel sobelXmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelX);
        MaskKernel sobelYmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelY);
        RawImage sobelXimage = ConvolutionGrayScale(smooth, sobelXmask);
        RawImage sobelYimage = ConvolutionGrayScale(smooth, sobelYmask);
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

        RawImage noiseImage = new(input.Width, input.Height);
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
    public static RawImage HistoEqualizeGrayscale(RawImage input) {
        int[] gray = new int[256];
        for (int i = 0; i < input.Length; i++) {
            uint value = input[i] >> R & 0xFF;
            gray[value]++;
        }
        int maxCdf = int.MinValue;
        int minCdf = int.MaxValue;
        int[] cdf = new int[256];
        cdf[0] = gray[0];
        for (int i = 1; i < 256; i++) {
            cdf[i] = gray[i] + cdf[i - 1];
            minCdf = Math.Min(minCdf, cdf[i]);
            maxCdf = Math.Max(maxCdf, cdf[i]);
        }
        if (minCdf == maxCdf) {
            return input;
        }
        double denominator = 255.0 / (maxCdf - minCdf);
        uint[] eqhisto = new uint[256];
        eqhisto[0] = 2;
        for (int i = 1; i < 256; i++) {
            eqhisto[i] = (uint)Math.Round((cdf[i] - minCdf) * denominator);
        }

        RawImage output = new(input.Width, input.Height);
        for (int i = 0; i < output.Length; i++) {
            uint inVal = input[i] >> R & 0xFF;
            uint outVal = eqhisto[inVal];
            uint pixel = outVal << R | outVal << G | outVal << B | 0xFF000000;

            output[i] = pixel;
        }
        return output;
    }
    public static RawImage HistoEqualizeFullColor(RawImage image) {
        throw new NotImplementedException();
    }

    // License Plate Detection
    public static RawImage RedChannel(RawImage input) {
        return GetChannel(input, R);
    }
    public static RawImage GreenChannel(RawImage input) {
        return GetChannel(input, G);
    }
    public static RawImage BlueChannel(RawImage input) {
        return GetChannel(input, B);
    }
    private static RawImage GetChannel(RawImage input, int channel) {
        RawImage output = new(input.Width, input.Height);
        int length = output.Length;
        Parallel.For(0, length, i => {
            uint pval = input[i] >> channel & 0xFF;
            output[i] = (pval << B | pval << G | pval << R | 0xFF000000);
        });
        return output;
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
    private static int[,] LocalAverageBrightness(RawImage input, int cellSize) {
        RawImage grayscale = GrayScale(input);
        int cellNumX = grayscale.Width / cellSize + 1;
        int cellNumY = grayscale.Height / cellSize + 1;
        int[,] averageBrightness = new int[cellNumX, cellNumY];
        for (int i = 0; i < grayscale.Width; i++) {
            for (int j = 0; j < grayscale.Height; j++) {
                int cellX = i / cellSize;
                int cellY = j / cellSize;
                averageBrightness[cellX, cellY] += (int)(grayscale.GetPixel(i, j) & 0xFF);
            }
        }
        for (int i = 0; i < averageBrightness.GetLength(0); i++) {
            for (int j = 0; j < averageBrightness.GetLength(1); j++) {
                averageBrightness[i, j] = (averageBrightness[i, j] / (cellSize * cellSize));
            }
        }
        return averageBrightness;
    }
    public static RawImage Mosaic(RawImage input, int cellSize) {
        int[,] averageBrightness = LocalAverageBrightness(input, cellSize);
        RawImage output = new(input.Width, input.Height);
        for (int i = 0; i < output.Width; i++) {
            for (int j = 0; j < output.Height; j++) {
                int cellX = i / cellSize;
                int cellY = j / cellSize;
                int pval = averageBrightness[cellX, cellY];
                uint pixel = ((uint)pval << B | (uint)pval << G | (uint)pval << R | 0xFF000000);
                output[i, j] = pixel;
            }
        }
        return output;
    }
    public static RawImage Outline(RawImage input) {
        // input is a binarized image
        RawImage output = new(input.Width, input.Height);
        for (int i = 1; i < input.Width-1; i++) {
            for (int j = 1; j < input.Height - 1; j++) {
                if ((int)(input[i, j] & 0xFF) == 0) continue;
                if ((int)(input[i - 1, j] & 0xFF) == 0) {
                    output[i, j] = 0xFFFFFFFF;
                    continue;
                }
                if ((int)(input[i + 1, j] & 0xFF) == 0) {
                    output[i, j] = 0xFFFFFFFF;
                    continue;
                }
                if ((int)(input[i, j - 1] & 0xFF) == 0) {
                    output[i, j] = 0xFFFFFFFF;
                    continue;
                }
                if ((int)(input[i, j + 1] & 0xFF) == 0) {
                    output[i, j] = 0xFFFFFFFF;
                    continue;
                }
            }
        }
        output = Reverse(output);
        return output;
    }
    public static RawImage BinarizeLocal(RawImage input, int cellSize) {
        RawImage localAvg = Mosaic(input, cellSize);
        RawImage output = new RawImage(input.Width, input.Height);
        int length = output.Length;
        Parallel.For(0, length, i => {
            uint inputVal = input[i] & 0xFF;
            uint threshold = localAvg[i] & 0xFF;
            if (inputVal > threshold) {
                output[i] = 0xFFFFFFFF;
            }
        });
        return output;
    }
    public static RawImage BinarizeGlobal(RawImage input, int threshold) {
        RawImage grayscale = GrayScale(input);
        RawImage output = new(input.Width, input.Height);
        int length = grayscale.Length;
        Parallel.For(0, length, i => {
            uint pval = input[i] & 0xFF;
            if (pval >= threshold) {
                output[i] = 0xFFFFFFFF;
            }
        });
        return output;
    }

}
