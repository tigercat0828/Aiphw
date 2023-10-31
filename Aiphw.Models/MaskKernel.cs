using System.Text;

namespace Aiphw.Models;

public enum DefaultMask {
    Custom, GaussianSmooth, SobelX, SobelY
}
public class MaskKernel {

    float[][] mask;
    public int Size => mask.Length;
    public float Scalar { get; private set; }
    public float[] this[int index] {
        get => mask[index];
        set => mask[index] = value;
    }
    public MaskKernel(float[] elements) {
        Scalar = Math.Max(elements.Sum(), 1);

        int size = (int)Math.Sqrt(elements.Length);
        mask = new float[size][];

        for (int i = 0; i < size; i++) {
            mask[i] = new float[size];
            Array.Copy(elements, i * size, mask[i], 0, size);
        }
    }
    public MaskKernel(int size) {

        mask = new float[size][];
        for (int i = 0; i < size; i++) {
            mask[i] = new float[size];
        }
    }
    public static MaskKernel LoadPreBuiltMask(DefaultMask mask) {
        switch (mask) {
            case DefaultMask.GaussianSmooth:
                return new MaskKernel(SmoothMask);
            case DefaultMask.SobelX:
                return new MaskKernel(SobelX);
            case DefaultMask.SobelY:
                return new MaskKernel(SobelY);
            default:
                break;
        }
        return new MaskKernel(Array.Empty<float>());
    }
    public void Transpose() {
        Array.Reverse(mask);
    }
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < mask.Length; i++) {
            sb.AppendLine("[ " + string.Join(", ", mask[i]) + "]");
        }
        return sb.ToString();
    }
    static float[] SmoothMask = {
        2, 4, 5, 4, 2,
        4, 9, 12, 9, 4,
        5, 12, 15, 12, 5,
        4, 9, 12, 9, 4,
        2, 4, 5, 4, 2
    };

    static float[] SobelX = {
        -1, 0,  1,
        -2, 0,  2,
        -1, 0,  1,
    };
    static float[] SobelY = {
        -1, -2, -1,
        0,  0,  0,
        1,  2,  1,
    };
}