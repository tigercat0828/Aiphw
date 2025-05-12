using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Aiphw.Models;
using ScottPlot;
using ScottPlot.Statistics;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
namespace Aiphw.WPF.Extensions;
public static class Utility {
    public static BitmapImage BitmapToImageSource(Bitmap bitmap) {
        using MemoryStream memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;
        BitmapImage bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();
        return bitmapimage;
    }
    public static void UpdateImageBox(Image imgBox, Bitmap bitmap) {
        imgBox.Source = BitmapToImageSource(bitmap);
    }
    public static void SetHistogram(Plot plot, double[] values, string xLabel, string yLabel, double min, double max, int binCount) {

        var hist = Histogram.WithBinCount(binCount, min, max); // Histogram hist = new(min, max, binCount);
        plot.Clear();
        hist.AddRange(values);
        plot.Add.Histogram(hist);   // plot.AddBar(values: hist.Counts, positions: hist.Bins);

        plot.XLabel(xLabel);
        plot.YLabel(yLabel);
        plot.Axes.SetLimits(bottom: 0);   // plot.SetAxisLimits(yMin: 0);
    }

    public static void SetHistogramFromChannel(Plot plot, RawImage image, int channel, Color color, string barLabel, bool clear = true) {

        double[] channels = new double[image.Pixels.Length];

        for (int i = 0; i < channels.Length; i++) {
            channels[i] = image.Pixels[i] >> (channel * 8) & 0xFF;
            //Console.WriteLine(channels[i]);
        }

        // var hist = Histogram.WithFixedBinSize(min: 0, max: 255, binSize: 1); 4.0
        var hist = Histogram.WithBinSize(1, 0, 255);


        if (clear) {
            plot.Clear();
        }
        hist.AddRange(channels);
        // var bar = plot.AddBar(values: hist.Counts, positions: hist.Bins, color);
        var bar = plot.Add.Bars(hist.Bins, hist.Counts);

        // bar.Label = barLabel;

        //var legend = plot.Legend(enable: true);
        //legend.Orientation = Orientation.Horizontal;
        //legend.Location = Alignment.UpperCenter;

        plot.XLabel("intensity");
        plot.YLabel("frequency");
        plot.Axes.SetLimits(bottom: 0);  //plot.SetAxisLimits(yMin: 0);
    }
}
