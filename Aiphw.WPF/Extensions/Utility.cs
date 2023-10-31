using Aiphw.Models;
using ScottPlot;
using ScottPlot.Statistics;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Aiphw.WPF.Extensions;
public static class Utility {
    public static BitmapImage BitmapToImageSource(Bitmap bitmap) {
        using (MemoryStream memory = new MemoryStream()) {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            return bitmapimage;
        }
    }
    public static void UpdateImageBox(Image imgBox, Bitmap bitmap) {
        imgBox.Source = BitmapToImageSource(bitmap);
    }
    public static void SetHistogram(Plot plot, double[] values, string xLabel, string yLabel, double min, double max, int binCount) {
        Histogram hist = new(min, max, binCount);
        plot.Clear();
        hist.AddRange(values);
        plot.AddBar(values: hist.Counts, positions: hist.Bins);

        plot.XAxis.Label(xLabel);
        plot.YAxis.Label(yLabel);

        plot.SetAxisLimits(yMin: 0);
    }
    public static void SetSingleChannelHistogram(Plot plot, RawImage2 image, int channel, Color color, string barLabel, bool clear = true) {

        double[] channels = new double[image.Pixels.Length / 4];

        for (int i = 0; i < channels.Length; i++) {
            channels[i] = image.Pixels[i * 4 + channel];
        }

        var hist = Histogram.WithFixedBinSize(min: 0, max: 255, binSize: 1);
        if (clear) {
            plot.Clear();
        }
        hist.AddRange(channels);
        var bar = plot.AddBar(values: hist.Counts, positions: hist.Bins, color);
        bar.BarWidth = 1;
        bar.Label = barLabel;

        var legend = plot.Legend(enable: true);
        legend.Orientation = Orientation.Horizontal;
        legend.Location = Alignment.UpperCenter;

        //plot.XAxis.Label("intensity");
        //plot.YAxis.Label("frequency");
        plot.SetAxisLimits(yMin: 0);
    }

    public static void SetHistogramFromChannel(Plot plot, RawImage image, int channel, Color color, string barLabel, bool clear = true) {

        double[] channels = new double[image.Pixels.Length];

        for (int i = 0; i < channels.Length; i++) {
            channels[i] = image.Pixels[i] >> (channel * 8) & 0xFF;
            //Console.WriteLine(channels[i]);
        }

        var hist = Histogram.WithFixedBinSize(min: 0, max: 255, binSize: 1);
        if (clear) {
            plot.Clear();
        }
        hist.AddRange(channels);
        var bar = plot.AddBar(values: hist.Counts, positions: hist.Bins, color);
        bar.BarWidth = 1;
        bar.Label = barLabel;

        var legend = plot.Legend(enable: true);
        legend.Orientation = Orientation.Horizontal;
        legend.Location = Alignment.UpperCenter;

        //plot.XAxis.Label("intensity");
        //plot.YAxis.Label("frequency");
        plot.SetAxisLimits(yMin: 0);
    }
}
